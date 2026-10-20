using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IBrandRepository Brands { get; }
        ICartRepository Carts { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
