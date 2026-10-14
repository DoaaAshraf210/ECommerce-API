using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParams query);
        Task<ProductDto> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task UpdateAsync(int id, UpdateProductDto dto);
        Task DeleteAsync(int id);
    }
}
