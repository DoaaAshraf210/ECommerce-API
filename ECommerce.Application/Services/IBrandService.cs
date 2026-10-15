using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Brands;
namespace ECommerce.Application.Services
{
    public interface IBrandService
    {
        Task<List<BrandDto>> GetAllAsync();
        Task<BrandDto> GetByIdAsync(int id);
        Task<BrandDto> CreateAsync(CreateBrandDto dto);
        Task UpdateAsync(int id, UpdateBrandDto dto);
        Task DeleteAsync(int id);
    }
}
