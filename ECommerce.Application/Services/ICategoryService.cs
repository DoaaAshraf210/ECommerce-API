using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface ICategoryService
    {

        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task UpdateAsync(int id, UpdateCategoryDto dto);
        Task DeleteAsync(int id);

    }
}
