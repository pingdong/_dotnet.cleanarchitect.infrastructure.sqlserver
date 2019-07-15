using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class GenericDbContext<TId> : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        
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
    }
}
