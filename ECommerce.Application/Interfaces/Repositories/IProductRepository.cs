using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface IProductRepository:IGenericRepository<Product>
    {

        Task<Product?> GetByIdWithDetailsAsync(int id);

        Task<PagedResult<Product>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            int? categoryId,
            int? brandId);

        Task<bool> SkuExistsAsync(string sku, int? excludeId = null);

        Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids);








    }
}
