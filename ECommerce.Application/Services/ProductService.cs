using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.Mappings;

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;

        public ProductService(IUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParams q)
        {
            var pageNumber = Math.Max(1, q.PageNumber);
            var pageSize = Math.Clamp(q.PageSize, 1, 50);

            var result = await _uow.Products.GetPagedAsync(
                pageNumber, pageSize, q.Search, q.CategoryId, q.BrandId);

            var items = result.Items.Select(p => p.ToDto()).ToList();
            return new PagedResult<ProductDto>(items, result.TotalCount, pageNumber, pageSize);
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _uow.Products.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException("Product", id);

            return product.ToDto();
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            await EnsureCategoryAndBrandExistAsync(dto.CategoryId, dto.BrandId);

            if (await _uow.Products.SkuExistsAsync(dto.Sku))
                throw new ConflictException($"A product with SKU '{dto.Sku}' already exists.");

            var product = dto.ToEntity();
            await _uow.Products.AddAsync(product);
            await _uow.SaveChangesAsync();

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
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product", id);

            _uow.Products.Delete(product);
            await _uow.SaveChangesAsync();
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