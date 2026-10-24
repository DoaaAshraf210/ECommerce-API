using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {

        public CartRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Cart?> GetByUserIdWithItemsAsync(Guid userId)
            => await _set
        .Include(c => c.Items)
            .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
        .AsSplitQuery()
        .FirstOrDefaultAsync(c => c.UserId == userId);


    }
}