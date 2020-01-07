using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public partial class GenericDbContext
    {
        /// <summary>
        /// An empty IMediator implementation
        /// The purpose of this Mediator is used to db migration of EF and some test scenarios.
        /// It should be used only in those scenarios.
        /// </summary>
        public class EmptyMediator : IMediator
        {
            public Task<object> Send(object request, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult(default(object));
            }

            public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.CompletedTask;
            }

            public Task Publish<TNotification>(TNotification notification,
                CancellationToken cancellationToken = default) where TNotification : INotification
            {
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(TResponse));
            }

            public Task Send(IRequest request, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
