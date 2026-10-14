using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class BrandRepository:GenericRepository<Brand>,IBrandRepository
       
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
                      => await _set.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));

    }
}
