using ECommerce.Application.Common.Caching;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using System;
using System.Collections.Generic;
using System.Text;


namespace ECommerce.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;

        public CategoryService(IUnitOfWork uow, ICacheService cache)
        {
            _uow = uow;
            _cache = cache;
        }
        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {

            if(await _uow.Categories.NameExistsAsync(dto.Name))
                throw new ConflictException($"A category with name '{dto.Name}' already exists.");

            var category = dto.ToEntity();
            await _uow.Categories.AddAsync(category);
            await _uow.SaveChangesAsync();
            await _cache.RemoveByTagAsync(CacheTags.Categories);
            return category.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
          

            var category = await _uow.Categories.GetByIdAsync(id)
       ?? throw new NotFoundException("Category", id);

            if (await _uow.Products.ExistsAsync(p => p.CategoryId == id))
                throw new ConflictException("Cannot delete a category that still has products.");

            _uow.Categories.Delete(category);
            await _uow.SaveChangesAsync();
            await _cache.RemoveByTagAsync(CacheTags.Categories);


        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {

         

            return await _cache.GetOrCreateAsync(
                   CacheKeys.Categories,
                   async ct =>
                   {
                       var categories = await _uow.Categories.GetAllAsync();

                       return categories.Select(c => c.ToDto()).ToList();


                   },
                   expiration: TimeSpan.FromMinutes(10),
                   tags: new[] { CacheTags.Categories });

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
            await _cache.RemoveByTagAsync(CacheTags.Categories);
            await _cache.RemoveByTagAsync(CacheTags.Products);



        }
    }
}
