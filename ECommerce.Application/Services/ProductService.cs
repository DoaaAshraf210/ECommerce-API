using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        public ProductService(IUnitOfWork uow, ICacheService cache)
        {
            _uow = uow;
            _cache = cache;

        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParams q)
        {
            var pageNumber = Math.Max(1, q.PageNumber);
            var pageSize = Math.Clamp(q.PageSize, 1, 50);
            var search = q.Search?.Trim().ToLowerInvariant();

            var key = CacheKeys.ProductList(pageNumber, pageSize, search, q.CategoryId, q.BrandId);

            return await _cache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    var result = await _uow.Products.GetPagedAsync(
                        pageNumber, pageSize, search, q.CategoryId, q.BrandId);

                    var items = result.Items.Select(p => p.ToDto()).ToList();
                    return new PagedResult<ProductDto>(items, result.TotalCount, pageNumber, pageSize);
                },
                expiration: TimeSpan.FromMinutes(2),
                tags: new[] { CacheTags.Products });
        }

        public Task<ProductDto> GetByIdAsync(int id)
            => _cache.GetOrCreateAsync(
                CacheKeys.Product(id),
                async ct =>
                {
                    var product = await _uow.Products.GetByIdWithDetailsAsync(id)
                        ?? throw new NotFoundException("Product", id);

                    return product.ToDto();
                },
                expiration: TimeSpan.FromMinutes(5),
                tags: new[] { CacheTags.Products });
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            await EnsureCategoryAndBrandExistAsync(dto.CategoryId, dto.BrandId);

            if (await _uow.Products.SkuExistsAsync(dto.Sku))
                throw new ConflictException($"A product with SKU '{dto.Sku}' already exists.");

            var product = dto.ToEntity();
            await _uow.Products.AddAsync(product);
            await _uow.SaveChangesAsync();
            await _cache.RemoveByTagAsync(CacheTags.Products);

            var created = await _uow.Products.GetByIdWithDetailsAsync(product.Id);
            return created!.ToDto();
        }

        public async Task UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _uow.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product", id);

            await EnsureCategoryAndBrandExistAsync(dto.CategoryId, dto.BrandId);

            if (await _uow.Products.SkuExistsAsync(dto.Sku, excludeId: id))
                throw new ConflictException($"A product with SKU '{dto.Sku}' already exists.");

            product.UpdateFrom(dto);
            await _uow.SaveChangesAsync();
            await _cache.RemoveByTagAsync(CacheTags.Products);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product", id);

            _uow.Products.Delete(product);
            await _uow.SaveChangesAsync();
            await _cache.RemoveByTagAsync(CacheTags.Products);
        }

        private async Task EnsureCategoryAndBrandExistAsync(int categoryId, int brandId)
        {
            if (!await _uow.Categories.ExistsAsync(c => c.Id == categoryId))
                throw new NotFoundException("Category", categoryId);

            if (!await _uow.Brands.ExistsAsync(b => b.Id == brandId))
                throw new NotFoundException("Brand", brandId);
        }
    }
}