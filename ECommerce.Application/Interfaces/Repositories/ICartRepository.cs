using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface ICartRepository:IGenericRepository<Cart>
    {
        Task<Cart?> GetByUserIdWithItemsAsync(Guid userId);

    }
}
