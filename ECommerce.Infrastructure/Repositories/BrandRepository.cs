using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
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
    }
}
