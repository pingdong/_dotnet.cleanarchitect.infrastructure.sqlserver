using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;
using PingDong.CleanArchitect.Core.Validation;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.UnitTests
{
    internal class TestRepository<TId, T> : IRepository<TId, T> where T : Entity<TId>, IAggregateRoot 
    {
        private readonly GenericDbContext<Guid> _context;
        private readonly IEnumerable<IValidator<T>> _validators;

        public TestRepository(GenericDbContext<Guid> context, IEnumerable<IValidator<T>> validators)
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

        public Task<T> FindByIdAsync(TId id, bool throwIfMissing = true)
        {
            throw new NotImplementedException();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<IList<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<IList<T>> ListAsync()
        {
            return await _context.Set<T>().ToListAsync().ConfigureAwait(false);
        }
        
        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!entity.IsTransient())
                return;

            _validators.Validate(entity);

            await _context.Set<T>().AddAsync(entity).ConfigureAwait(false);
        }

        public async Task AddAsync(IList<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (!entities.Any())
                return;

            foreach (var entity in entities)
            {
                await AddAsync(entity);
            }
        }

        public async Task RemoveAsync(TId id)
        {
            if(EqualityComparer<TId>.Default.Equals(id, default)) 
                throw new ArgumentNullException(nameof(id));

            var entity = await _context.Set<T>().FindAsync(id).ConfigureAwait(false);
            if (null == entity)
                throw new ArgumentOutOfRangeException(nameof(id));

            _context.Set<T>().Remove(entity);
        }

        public async Task RemoveAsync(IList<TId> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (!ids.Any())
                return;

            foreach (var id in ids)
            {
                await RemoveAsync(id);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.IsTransient())
                throw new ArgumentException(nameof(entity));
            
            var existing = await _context.Set<T>().FindAsync(entity.Id).ConfigureAwait(false);
            if (existing == null)
                throw new ArgumentOutOfRangeException(nameof(entity));

            _validators.Validate(entity);
            
            _context.Update(entity);
        }

        public async Task UpdateAsync(IList<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (!entities.Any())
                return;

            foreach (var entity in entities)
            {
                await UpdateAsync(entity);
            }
        }

        #endregion
    }
}
