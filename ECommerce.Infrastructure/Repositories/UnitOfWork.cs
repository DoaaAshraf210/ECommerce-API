using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        
        public IProductRepository Products { get;  }
        public ICategoryRepository Categories { get;  }
        public IBrandRepository Brands { get; }

        public ICartRepository Carts { get; }

        public UnitOfWork(ApplicationDbContext context,
        IProductRepository products,
        ICategoryRepository categories,
        IBrandRepository brands,
        ICartRepository carts)
        {
            _context = context;
            Products = products;
            Categories = categories;
            Brands = brands;
            Carts = carts;
        }

    
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => _context.SaveChangesAsync(cancellationToken);
    }
}
