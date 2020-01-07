using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;
using PingDong.CleanArchitect.Infrastructure;
using PingDong.CleanArchitect.Infrastructure.SqlServer;
using PingDong.Validation;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class GenericRepository<TId, T> : IRepository<TId, T> where T : Entity<TId>, IAggregateRoot 
    {
        public GenericRepository(GenericDbContext<TId> context)
        {
            DbContext = context;
        }

        #region IRepository
        
        /// <inheritdoc />
        public IUnitOfWork UnitOfWork => DbContext;
        
        /// <inheritdoc />
        public async Task<IList<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbContext.Set<T>().Where(predicate).ToListAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> FindByIdAsync(TId id, bool throwIfMissing = true)
        {
            var found = await DbContext.Set<T>().FindAsync(id).ConfigureAwait(false);
            if (throwIfMissing)
                found.ThrowIfMissing();

            return found;
        }

        /// <inheritdoc />
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbContext.Set<T>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IList<T>> ListAsync()
        {
            return await DbContext.Set<T>().ToListAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task AddAsync(T entity)
        {
            await NullValidateAndExecuteAsync(
                entity,
                async context => { await DbContext.Set<T>().AddAsync(entity).ConfigureAwait(false); }
            ).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task AddAsync(IList<T> entities)
        {
            await ExecuteInTransactionAsync(context =>
            {
                DbContext.Set<T>().AddRange(entities);
            }).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task RemoveAsync(TId id)
        {
            await ExecuteInTransactionAsync(async context =>
            {
                var entity = await DbContext.Set<T>().FindAsync(id).ConfigureAwait(false);

                DbContext.Set<T>().Remove(entity);
            }).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task RemoveAsync(IList<TId> entityIds)
        {
            await ExecuteInTransactionAsync(context =>
            {
                var entities = DbContext.Set<T>().Where(entity => entityIds.Contains(entity.Id));

                DbContext.Set<T>().RemoveRange(entities);
            }).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task UpdateAsync(IList<T> entities)
        {
            await ExecuteInTransactionAsync(async context =>
            {
                foreach (var entity in entities)
                {
                    var existing = await context.Set<T>().FindAsync(entity.Id).ConfigureAwait(false);
                    existing.ThrowIfMissing();

                    context.Entry(existing).CurrentValues.SetValues(entity);
                }
            }).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task UpdateAsync(T entity)
        {
            await NullValidateAndExecuteAsync(
                entity,
                async context =>
                {
                    var existing = await context.Set<T>().FindAsync(entity.Id).ConfigureAwait(false);
                    existing.ThrowIfMissing();

                    context.Entry(existing).CurrentValues.SetValues(entity);
                }
            ).ConfigureAwait(false);
        }

        #endregion

        #region Protected

        protected GenericDbContext<TId> DbContext { get; }

        protected async Task ExecuteAsyncInTransactionAsync(Func<GenericDbContext<TId>, Task> func)
        {
            var strategy = DbContext.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                if (DbContext.Database.IsSqlServer())
                {
                    using (var transaction = DbContext.Database.BeginTransaction())
                    {
                        await func(DbContext).ConfigureAwait(false);

                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                        transaction.Commit();
                    }
                }
                else
                {
                    await func(DbContext).ConfigureAwait(false);

                    await DbContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        protected async Task ExecuteInTransactionAsync(Action<GenericDbContext<TId>> action)
        {
            var strategy = DbContext.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                if (DbContext.Database.IsSqlServer())
                {
                    using (var transaction = DbContext.Database.BeginTransaction())
                    {
                        action(DbContext);

                        await DbContext.SaveChangesAsync().ConfigureAwait(false);

                        transaction.Commit();
                    }
                }
                else
                {
                    action(DbContext);

                    await DbContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        protected async Task NullValidateAndExecuteAsync(T entity, Func<GenericDbContext<TId>, Task> func)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await func(DbContext).ConfigureAwait(false);
            
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        protected async Task NullValidateAndExecuteAsync(T entity, Action<GenericDbContext<TId>> action)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            action(DbContext);

            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
