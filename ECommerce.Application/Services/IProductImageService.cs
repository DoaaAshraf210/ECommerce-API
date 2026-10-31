using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Services;

public interface IProductImageService
{
    Task<List<ProductImageDto>> AddAsync(int productId, IReadOnlyList<UploadedFile> files, CancellationToken ct = default);
    Task SetMainAsync(int productId, int imageId, CancellationToken ct = default);
    Task DeleteAsync(int productId, int imageId, CancellationToken ct = default);
}