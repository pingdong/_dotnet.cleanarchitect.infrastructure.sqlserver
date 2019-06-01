using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PingDong.CleanArchitect.Core;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class GenericRepository<TId, T> : IRepository<TId, T> where T : Entity<TId>, IAggregateRoot 
    {
        private readonly GenericDbContext _context;
        private readonly IEnumerable<IValidator<T>> _validators;

        public GenericRepository(GenericDbContext context, IEnumerable<IValidator<T>> validators)
        {
            _context = context;
            _validators = validators;
        }

        #region IRepository
        
        public IUnitOfWork UnitOfWork => _context;

        public async Task<T> FindByIdAsync(TId id)
        {
            return await _context.Set<T>().FindAsync(id).ConfigureAwait(false);
        }
        
        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _validators.Validate(entity);

            if (entity.IsTransient())
                await _context.Set<T>().AddAsync(entity).ConfigureAwait(false);
        }
        
        public async Task RemoveAsync(TId id)
        {
            if(EqualityComparer<TId>.Default.Equals(id, default(TId))) 
                throw new ArgumentNullException(nameof(id));

            var entity = await _context.Set<T>().FindAsync(id).ConfigureAwait(false);
            if (null == entity)
                throw new ArgumentOutOfRangeException(nameof(id));

            _context.Set<T>().Remove(entity);
        }
        
        public Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            _context.Update(entity);

            return Task.CompletedTask;
        }

        #endregion
    }
}
