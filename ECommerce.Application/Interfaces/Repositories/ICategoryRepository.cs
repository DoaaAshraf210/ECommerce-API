using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface ICategoryRepository:IGenericRepository<Category>
    {

        Task<bool> NameExistsAsync(string name, int? excludeId = null);

    }
}
