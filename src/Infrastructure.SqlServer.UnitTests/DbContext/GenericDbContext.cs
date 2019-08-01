using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moq;
using PingDong.CleanArchitect.Core;
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
                var entity = new TestEntity { Name = "Test" };
                entity.AddDomainEvent(new TestDomainEvent());

                await repository.AddAsync(entity);
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
                var entity = new TestEntity { Name = "Test" };
                
                await repository.AddAsync(entity);
                await repository.UnitOfWork.SaveEntitiesAsync();

                Assert.True(dbContext.IsSaveChangesTriggered);
            });

            mock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private async void ExecuteTestCase(IMediator mediator, Func<TestRepository<Guid, TestEntity>, TestDbContext, Task> action)
        {
            var options = new DbContextOptionsBuilder<GenericDbContext<Guid>>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new TestDbContext(options, mediator))
            {
                await context.Database.EnsureCreatedAsync();

                var repository = new TestRepository<Guid, TestEntity>(context, null);

                await action(repository, context);
            }
        }
    }

    internal class TestDomainEvent : DomainEvent
    {

    }

    internal class TestDbContext : GenericDbContext<Guid>
    {
        public TestDbContext(DbContextOptions options, IMediator mediator) : base(options, mediator) {}
        
        public DbSet<TestEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client Requests
            modelBuilder.ApplyConfiguration(new TestEntityTypeConfiguration());
        }  

        public bool IsSaveChangesTriggered { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsSaveChangesTriggered = true;

            return base.SaveChangesAsync(cancellationToken);
        }
    }

    internal class TestEntity : Entity<Guid>, IAggregateRoot
    {
        public string Name { get; set; }
    }
    internal class TestEntityTypeConfiguration : IEntityTypeConfiguration<TestEntity>
    {
        private readonly string _schema = "dbo";
        private readonly string _tableName = "entities";

        public void Configure(EntityTypeBuilder<TestEntity> configuration)
        {
            configuration.ToTable(_tableName, _schema);

            configuration.HasKey(cr => cr.Id);

            configuration.Property(b => b.Id)
                            .HasColumnType("uniqueidentifier")
                            .IsRequired();
            configuration.Property(cr => cr.Name)
                            .HasColumnType("nvarchar(40)")
                            .IsRequired();

            configuration.Ignore(c => c.DomainEvents);
        }
    }
}
