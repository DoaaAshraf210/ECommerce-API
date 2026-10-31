using ECommerce.Application.Common;
using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class ProductImageService : IProductImageService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorage _storage;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductImageService> _logger;

    public ProductImageService(
        IUnitOfWork uow, IFileStorage storage, ICacheService cache,
        ILogger<ProductImageService> logger)
    {
        _uow = uow;
        _storage = storage;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProductImageDto>> AddAsync(
        int productId, IReadOnlyList<UploadedFile> files, CancellationToken ct = default)
    {
        if (files.Count == 0)
            throw new BadRequestException("No files were uploaded.");

        if (files.Count > ProductImageRules.MaxFilesPerRequest)
            throw new BadRequestException(
                $"You can upload at most {ProductImageRules.MaxFilesPerRequest} images at once.");

        var product = await _uow.Products.GetByIdWithDetailsAsync(productId)
            ?? throw new NotFoundException("Product", productId);

        if (product.Images.Count + files.Count > ProductImageRules.MaxImagesPerProduct)
            throw new ConflictException(
                $"A product can have at most {ProductImageRules.MaxImagesPerProduct} images.");

        var extensions = new List<string>();
        foreach (var file in files)
            extensions.Add(await ValidateAsync(file, ct));

        var savedUrls = new List<string>();
        try
        {
            var firstIsMain = product.Images.Count == 0;

            for (var i = 0; i < files.Count; i++)
            {
                await using var stream = files[i].OpenRead();
                var url = await _storage.SaveAsync($"products/{productId}", extensions[i], stream, ct);
                savedUrls.Add(url);

                product.Images.Add(new ProductImage { Url = url, IsMain = firstIsMain && i == 0 });
            }

            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var url in savedUrls)
                await _storage.DeleteAsync(url, CancellationToken.None);
            throw;
        }

        await _cache.RemoveByTagAsync(CacheTags.Products, ct);

        _logger.LogInformation(
            "Added {Count} image(s) to product {ProductId}", savedUrls.Count, productId);

        return product.Images
            .Where(i => savedUrls.Contains(i.Url))
            .OrderBy(i => i.Id)
            .Select(i => i.ToDto())
            .ToList();
    }

    public async Task SetMainAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdWithDetailsAsync(productId)
            ?? throw new NotFoundException("Product", productId);

        if (product.Images.All(i => i.Id != imageId))
            throw new NotFoundException("Image", imageId);

        foreach (var image in product.Images)
            image.IsMain = image.Id == imageId;

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveByTagAsync(CacheTags.Products, ct);
    }

    public async Task DeleteAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdWithDetailsAsync(productId)
            ?? throw new NotFoundException("Product", productId);

        var image = product.Images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException("Image", imageId);

        var wasMain = image.IsMain;
        var url = image.Url;

        product.Images.Remove(image);

        if (wasMain)
        {
            var next = product.Images.OrderBy(i => i.Id).FirstOrDefault();
            if (next is not null)
                next.IsMain = true;
        }

        await _uow.SaveChangesAsync(ct);                       
        await _storage.DeleteAsync(url, CancellationToken.None); 
        await _cache.RemoveByTagAsync(CacheTags.Products, ct);
    }

    private static async Task<string> ValidateAsync(UploadedFile file, CancellationToken ct)
    {
        if (file.Length <= 0)
            throw new BadRequestException("One of the files is empty.");

        if (file.Length > ProductImageRules.MaxFileBytes)
            throw new BadRequestException("Each image must be 5 MB or smaller.");

        await using var stream = file.OpenRead();
        var header = new byte[12];
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);

        return ImageSniffer.DetectExtension(header.AsSpan(0, read))
            ?? throw new BadRequestException("Only JPEG, PNG and WebP images are allowed.");
    }
}