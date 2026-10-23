using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public IBrandRepository Brands { get; }

        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }

        public UnitOfWork(ApplicationDbContext context,
        IProductRepository products,
        ICategoryRepository categories,
        IBrandRepository brands,
        ICartRepository carts,
        IOrderRepository orders)
        {
            _context = context;
            Products = products;
            Categories = categories;
            Brands = brands;
            Carts = carts;
            Orders = orders;
        }


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "Some items were just updated by another request. Please review and try again.");
            }
        }
    }
}
