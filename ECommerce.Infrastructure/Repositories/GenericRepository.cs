using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Common;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace ECommerce.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _set;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
            => await _set.FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<T>> GetAllAsync()
            => await _set.AsNoTracking().ToListAsync();

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _set.AnyAsync(predicate);

        public async Task AddAsync(T entity)
            => await _set.AddAsync(entity);

        //public void Update(T entity)
        //    => _set.Update(entity);

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}