using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync<T>(this IMediator mediator, T context) where T: DbContext
        {
            var domainEntities = context.ChangeTracker
                                    .Entries<Entity<T>>()
                                    .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                                    .ToList();

            var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents)
                                             .ToList();

            domainEntities.ToList()
                          .ForEach(entity => entity.Entity.ClearDomainEvents());

            var tasks = domainEvents.Select(async domainEvent => {
                                                await mediator.Publish(domainEvent);
                                            });

            await Task.WhenAll(tasks);
        }
    }
}
