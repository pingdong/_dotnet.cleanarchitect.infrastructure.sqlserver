using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    internal static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync<T, TId>(this IMediator mediator, T context) where T: DbContext
        {
            var domainEntities = context.ChangeTracker
                                    .Entries<Entity<TId>>()
                                    .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                                    .ToList();

            var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents)
                                             .ToList();

            var tasks = domainEvents.Select(async domainEvent => {
                                                await mediator.Publish(domainEvent);
                                            });

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            await Task.WhenAll(tasks);
        }
    }
}
