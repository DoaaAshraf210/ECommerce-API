using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Brands;

namespace ECommerce.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _uow;
        public BrandService(IUnitOfWork uow)
        => _uow = uow;
        
        public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
        {

            if (await _uow.Brands.NameExistsAsync(dto.Name))
                throw new ConflictException($"A brand with name '{dto.Name}' already exists.");

            var brand = dto.ToEntity();
            await _uow.Brands.AddAsync(brand);
            await _uow.SaveChangesAsync();
            return brand.ToDto();
        }

        public async Task DeleteAsync(int id)
        {

            var brand = await _uow.Brands.GetByIdAsync(id)
       ?? throw new NotFoundException("Brand", id);

            if (await _uow.Products.ExistsAsync(p => p.BrandId == id))
                throw new ConflictException("Cannot delete a brand that still has products.");

            _uow.Brands.Delete(brand);
            await _uow.SaveChangesAsync();


        }

        public async Task<List<BrandDto>> GetAllAsync()
        {

            var brands = await _uow.Brands.GetAllAsync();

            return brands.Select(b => b.ToDto()).ToList();




        }

        public async Task<BrandDto> GetByIdAsync(int id)
        {
            var brand = await _uow.Brands.GetByIdAsync(id)
                ?? throw new NotFoundException("Brand", id);

            return brand.ToDto();
        }

        public async Task UpdateAsync(int id, UpdateBrandDto dto)
        {
            var brand = await _uow.Brands.GetByIdAsync(id)
               ?? throw new NotFoundException("Brand", id);

            if (await _uow.Brands.NameExistsAsync(dto.Name, excludeId: id))
                throw new ConflictException($"A brand with name '{dto.Name}' already exists.");


            brand.UpdateFrom(dto);
            await _uow.SaveChangesAsync();


        }
    }
}
