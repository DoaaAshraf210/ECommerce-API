using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.Mappings;
using ECommerce.Application.DTOs.Categories;


namespace ECommerce.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        public CategoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {

            if(await _uow.Categories.NameExistsAsync(dto.Name))
                throw new ConflictException($"A category with name '{dto.Name}' already exists.");

            var category = dto.ToEntity();
            await _uow.Categories.AddAsync(category);
            await _uow.SaveChangesAsync();
            return category.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
          
            var category=await _uow.Categories.GetByIdAsync(id) ?? throw new NotFoundException("Category", id);

            _uow.Categories.Delete(category);
            await _uow.SaveChangesAsync();



        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {

         var categories=   await _uow.Categories.GetAllAsync();

            return categories.Select(c => c.ToDto()).ToList();




        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id)
                ?? throw new NotFoundException("Category", id);

            return category.ToDto();
        }

        public async Task UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _uow.Categories.GetByIdAsync(id)
               ?? throw new NotFoundException("Category", id);

            if (await _uow.Categories.NameExistsAsync(dto.Name,excludeId: id))
                throw new ConflictException($"A category with name '{dto.Name}' already exists."); 


            category.UpdateFrom(dto);
            await _uow.SaveChangesAsync();


        }
    }
}
