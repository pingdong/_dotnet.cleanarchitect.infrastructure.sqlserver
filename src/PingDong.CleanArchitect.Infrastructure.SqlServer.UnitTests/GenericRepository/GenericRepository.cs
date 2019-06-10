using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingDong.CleanArchitect.Core;
using Xunit;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.UnitTests.GenericRepository
{
    public class GenericRepositoryTests
    {
        [Fact]
        public void Repository_FindById_ShouldReturnNull_IfNotExisted()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                var found = await repository.FindByIdAsync(Guid.NewGuid());
                Assert.Null(found);
            });
        }

        [Fact]
        public void Repository_FindById_ShouldReturn_IfExisted()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                var saved = await dbContext.Requests.FirstOrDefaultAsync();

                var found = await repository.FindByIdAsync(saved.Id);
                Assert.NotNull(found);
                Assert.Equal("Test", found.Name);
            });
        }

        [Fact]
        public void Repository_RemoveThrowException_IfValueNotExisted()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.RemoveAsync(Guid.NewGuid()));
            });
        }

        [Fact]
        public void Repository_Remove_IfValueExisted()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                Assert.Equal(1, await dbContext.Requests.CountAsync());

                var saved = await dbContext.Requests.FirstOrDefaultAsync();

                await repository.RemoveAsync(saved.Id);
                await repository.UnitOfWork.SaveEntitiesAsync();
                
                Assert.Equal(0, await dbContext.Requests.CountAsync());
            });
        }
        
        [Fact]
        public void Repository_UpdateThrowException_IfValueIsNew()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await Assert.ThrowsAsync<ArgumentException>(() => repository.UpdateAsync(request));
            });
        }
        
        [Fact]
        public void Repository_UpdateThrowException_IfValueNotExisted()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("Test");
                request.SetId(Guid.NewGuid());

                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.UpdateAsync(request));
            });
        }

        [Fact]
        public void Repository_UpdateThrowException_IfValueIsInvalid()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                var saved = await dbContext.Requests.FirstOrDefaultAsync();
                saved.Name = "NA";

                await Assert.ThrowsAsync<ValidationException>(() => repository.UpdateAsync(saved));
            });
        }

        [Fact]
        public void Repository_UpdateExistingValue_IfValid()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                var saved = await dbContext.Requests.FirstOrDefaultAsync();
                saved.Name = "Updated";

                await repository.UpdateAsync(saved);
                await repository.UnitOfWork.SaveEntitiesAsync();
                
                var updated = await dbContext.Requests.FirstOrDefaultAsync();
                Assert.Equal("Updated", updated.Name);
            });
        }
        
        [Fact]
        public void Repository_UpdateThrowException_IfValueIsNull()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
            });
        }

        [Fact]
        public void Repository_AddValue_IfValid()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("Test");

                await repository.AddAsync(request);
                await repository.UnitOfWork.SaveEntitiesAsync();

                Assert.Equal(1, await dbContext.Requests.CountAsync());
            });
        }
        
        [Fact]
        public void Repository_AddThrowException_IfValueIsInvalid()
        {
            var validators = new List<IValidator<Request>> {new RequestValidator()};

            ExecuteTestCase(validators, async (repository, dbContext) =>
            {
                var request = new Request("ab");

                await Assert.ThrowsAsync<ValidationException>(() => repository.AddAsync(request));
            });
        }
        
        [Fact]
        public void Repository_AddThrowException_IfValueIsNull()
        {
            ExecuteTestCase(null, async (repository, dbContext) =>
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));
            });
        }

        private async void ExecuteTestCase(IEnumerable<IValidator<Request>> validators, Func<GenericRepository<Guid, Request>, RequestDbContext, Task> action)
        {
            var options = new DbContextOptionsBuilder<GenericDbContext<Guid>>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new RequestDbContext(options, new EmptyMediator()))
            {
                await context.Database.EnsureCreatedAsync();

                var repository = new GenericRepository<Guid, Request>(context, validators);

                await action(repository, context);
            }
        }
    }

    internal class RequestDbContext : GenericDbContext<Guid>
    {
        public RequestDbContext(DbContextOptions options, IMediator mediator) : base(options, mediator) {}

        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RequestEntityTypeConfiguration());
        }
    }

    internal class Request: Entity<Guid>, IAggregateRoot
    {public Request(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public void SetId(Guid id)
        {
            Id = id;
        }
    }
    
    internal class RequestEntityTypeConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> requestConfiguration)
        {
            requestConfiguration.ToTable("Requests");

            requestConfiguration.HasKey(cr => cr.Id);
            requestConfiguration.Property(cr => cr.Name).IsRequired();
        }
    }

    internal class RequestValidator: AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(a => a.Name).Length(3, 10);
        }
    }
}
