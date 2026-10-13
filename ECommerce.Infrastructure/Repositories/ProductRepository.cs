using ECommerce.Application.Common.Models;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context):base(context)
        {
            
        }
        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        => await _set
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PagedResult<Product>> GetPagedAsync(int pageNumber, int pageSize, string? search, int? categoryId, int? brandId)
        {
            var query = _set
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, pageNumber, pageSize);
        }


        public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        => await _set.AnyAsync(p => p.Sku == sku && (excludeId == null || p.Id != excludeId));
    }
}
