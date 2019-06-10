using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;
using PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class GenericDbContext<TId> : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        
        protected GenericDbContext(DbContextOptions options) : base(options) { }

        public GenericDbContext(DbContextOptions options, IMediator mediator) : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            if (_mediator != null)
                await _mediator.DispatchDomainEventsAsync<GenericDbContext<TId>, TId>(this).ConfigureAwait(false);

            await SaveChangesAsync(cancellationToken);

            return true;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration<TId>(RequestsManagerTable.DefaultSchema));
        }
    }
}
