using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency;
using PingDong.CleanArchitect.Service;
using Xunit;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.UnitTests
{
    public class GenericDbContextTests
    {
        [Fact]
        public void SaveEntity_ThrowException_IfMediatorIsNull()
        {
            var options = new DbContextOptionsBuilder<GenericDbContext<Guid>>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Assert.Throws<ArgumentNullException>(() => new GenericDbContext<Guid>(options, null));
        }
        
        [Fact]
        public void SaveEntity_ThrowException_IfOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericDbContext<Guid>(null, null));
        }
        
        [Fact]
        public void SaveEntity_DomainEventDispatched_IfDomainEventsProvided()
        {
            var mock = new Mock<IMediator>();
            mock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            ExecuteTestCase(mock.Object, async (repository, dbContext) =>
            {
                var request = new ClientRequest<Guid>("Test", DateTime.Now);
                request.AddDomainEvent(new TestDomainEvent());

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                Assert.True(dbContext.IsSaveChangesTriggered);
            });

            mock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public void SaveEntity_DomainEventDispatched_IfDomainEventsNotProvided()
        {
            var mock = new Mock<IMediator>();
            mock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            ExecuteTestCase(mock.Object, async (repository, dbContext) =>
            {
                var request = new ClientRequest<Guid>("Test", DateTime.Now);

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                Assert.True(dbContext.IsSaveChangesTriggered);
            });

            mock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private async void ExecuteTestCase(IMediator mediator, Func<TestRepository<Guid, ClientRequest<Guid>>, TestDbContext, Task> action)
        {
            var options = new DbContextOptionsBuilder<GenericDbContext<Guid>>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new TestDbContext(options, mediator))
            {
                await context.Database.EnsureCreatedAsync();

                var repository = new TestRepository<Guid, ClientRequest<Guid>>(context, null);

                await action(repository, context);
            }
        }
    }

    internal class TestDomainEvent : INotification
    {

    }

    internal class TestDbContext : GenericDbContext<Guid>
    {
        public TestDbContext(DbContextOptions options, IMediator mediator) : base(options, mediator) {}
        
        public DbSet<ClientRequest<Guid>> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client Requests
            modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration<Guid>());
        }  

        public bool IsSaveChangesTriggered { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsSaveChangesTriggered = true;

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
