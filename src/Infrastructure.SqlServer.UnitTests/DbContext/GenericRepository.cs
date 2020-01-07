using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PingDong.CleanArchitect.Core;
using PingDong.CleanArchitect.Core.Validation;
using Xunit;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.UnitTests
{
    public class GenericRepositoryTest
    {
        [Fact]
        public void FirstOrDefault()
        {
            ExecuteTestCase(async repository =>
            {
                var c1 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C1"
                };
                var c2 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C2"
                };

                await repository.AddAsync(new []{c1, c2});

                var c = await repository.FirstOrDefaultAsync(s => s.Name == "C2");
                Assert.Equal(c.Id, c2.Id);
                
                var cn = await repository.FirstOrDefaultAsync(s => s.Name == "C3");
                Assert.Null(cn);
            });
        }

        [Fact]
        public void FindById()
        {
            ExecuteTestCase(async repository =>
            {
                var c1 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C1"
                };
                var c2 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C2"
                };

                await repository.AddAsync(new []{c1, c2});

                var c = await repository.FindByIdAsync(c1.Id);
                Assert.Equal(c.Name, c1.Name);
                
                var cn = await repository.FindByIdAsync(Guid.NewGuid());
                Assert.Null(cn);
            });
        }
        
        [Fact]
        public void List()
        {
            ExecuteTestCase(async repository =>
            {
                var c1 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C1"
                };
                var c2 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C2"
                };

                await repository.AddAsync(new []{c1, c2});

                var c = await repository.ListAsync();
                Assert.Equal(2, c.Count);
                
                Assert.Equal(c1.Name, c.First(s => s.Id == c1.Id).Name);
                Assert.Equal(c2.Name, c.First(s => s.Id == c2.Id).Name);
            });
        }

        [Fact]
        public void Add()
        {
            ExecuteTestCase(async repository =>
            {
                var c1 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C1",
                    City = "city",
                    Address = "local",
                    PhoneNo = "00",
                    ContactPerson = "John",
                    ContactPersonPhoneNo = "01",
                    Website = "a.com",
                    Domain = "a.com",
                    Notes = "n",
                    Status = ObjectStatus.Pending
                };

                var s1 = new Subscription
                {
                    Id = Guid.NewGuid(),
                    CompanyId = c1.Id,
                    Name = "S1",
                    TenantId = Guid.NewGuid(),
                    TenantName = "T1",
                    Status = ObjectStatus.Pending
                };
                c1.Subscriptions.Add(s1);

                var u1 = new User
                {
                    Id = Guid.NewGuid(),
                    CompanyId = c1.Id,
                    Role = "R",
                    FirstName = "FN",
                    Surname = "SN",
                    Email = "mail",
                    Phone = "12",
                    Notes = "NU",
                    Status = ObjectStatus.Pending
                };
                c1.Users.Add(u1);

                await repository.AddAsync(c1);

                var c = await repository.FindByIdAsync(c1.Id);
                Assert.NotNull(c);
                
                Assert.Equal(c1.Name, c.Name);
                Assert.Equal(c1.City, c.City);
                Assert.Equal(c1.Address, c.Address);
                Assert.Equal(c1.PhoneNo, c.PhoneNo);
                Assert.Equal(c1.ContactPerson, c.ContactPerson);
                Assert.Equal(c1.ContactPersonPhoneNo, c.ContactPersonPhoneNo);
                Assert.Equal(c1.Website, c.Website);
                Assert.Equal(c1.Domain, c.Domain);
                Assert.Equal(c1.Notes, c.Notes);
                Assert.Equal(c1.Status, c.Status);

                Assert.Single(c.Subscriptions);
                Assert.Equal(s1.Id, c.Subscriptions[0].Id);
                Assert.Equal(s1.CompanyId, c.Subscriptions[0].CompanyId);
                Assert.Equal(s1.Name, c.Subscriptions[0].Name);
                Assert.Equal(s1.TenantId, c.Subscriptions[0].TenantId);
                Assert.Equal(s1.TenantName, c.Subscriptions[0].TenantName);
                Assert.Equal(s1.Status, c.Subscriptions[0].Status);
                
                Assert.Single(c.Users);
                Assert.Equal(u1.Id, c.Users[0].Id);
                Assert.Equal(u1.CompanyId, c.Users[0].CompanyId);
                Assert.Equal(u1.Role, c.Users[0].Role);
                Assert.Equal(u1.FirstName, c.Users[0].FirstName);
                Assert.Equal(u1.Surname, c.Users[0].Surname);
                Assert.Equal(u1.Email, c.Users[0].Email);
                Assert.Equal(u1.Phone, c.Users[0].Phone);
                Assert.Equal(u1.Notes, c.Users[0].Notes);
                Assert.Equal(u1.Status, c.Users[0].Status);
            });
        }
        
        [Fact]
        public void Remove()
        {
            ExecuteTestCase(async repository =>
            {
                var c1 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C1"
                };
                var c2 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C2"
                };
                var c3 = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "C3"
                };

                await repository.AddAsync(new []{c1, c2, c3});
                
                await repository.RemoveAsync(new [] {c2, c3});
                var cr1 = await repository.ListAsync();
                Assert.Single(cr1);
                Assert.Equal(c1.Name, cr1.First(s => s.Id == c1.Id).Name);
                
                await repository.RemoveAsync(c1);
                var cr2 = await repository.ListAsync();
                Assert.Empty(cr2);
            });
        }
        
        [Fact]
        public void Update()
        {
            ExecuteTestCase(async repository =>
            {
                var id = Guid.NewGuid();

                var co = new Company
                {
                    Id = id,
                    Name = "C",
                };
                await repository.AddAsync(co);

                var c1 = new Company
                {
                    Id = id,
                    Name = "C",
                };
                c1.City = "city";
                c1.Address = "local";
                c1.PhoneNo = "00";
                c1.ContactPerson = "John";
                c1.ContactPersonPhoneNo = "01";
                c1.Website = "a.com";
                c1.Domain = "a.com";
                c1.Notes = "n";
                c1.Status = ObjectStatus.Pending;

                var s1 = new Subscription
                {
                    Id = Guid.NewGuid(),
                    CompanyId = c1.Id,
                    Name = "S1",
                    TenantId = Guid.NewGuid(),
                    TenantName = "T1",
                    Status = ObjectStatus.Pending
                };
                c1.Subscriptions.Add(s1);

                var u1 = new User
                {
                    Id = Guid.NewGuid(),
                    CompanyId = c1.Id,
                    Role = "R",
                    FirstName = "FN",
                    Surname = "SN",
                    Email = "mail",
                    Phone = "12",
                    Notes = "NU",
                    Status = ObjectStatus.Pending
                };
                c1.Users.Add(u1);

                await repository.UpdateAsync(c1);

                var c = await repository.FindByIdAsync(id);
                Assert.NotNull(c);
                
                Assert.Equal(c1.Name, c.Name);
                Assert.Equal(c1.City, c.City);
                Assert.Equal(c1.Address, c.Address);
                Assert.Equal(c1.PhoneNo, c.PhoneNo);
                Assert.Equal(c1.ContactPerson, c.ContactPerson);
                Assert.Equal(c1.ContactPersonPhoneNo, c.ContactPersonPhoneNo);
                Assert.Equal(c1.Website, c.Website);
                Assert.Equal(c1.Domain, c.Domain);
                Assert.Equal(c1.Notes, c.Notes);
                Assert.Equal(c1.Status, c.Status);

                //Assert.Single(c.Subscriptions);
                //Assert.Equal(s1.Id, c.Subscriptions[0].Id);
                //Assert.Equal(s1.CompanyId, c.Subscriptions[0].CompanyId);
                //Assert.Equal(s1.Name, c.Subscriptions[0].Name);
                //Assert.Equal(s1.TenantId, c.Subscriptions[0].TenantId);
                //Assert.Equal(s1.TenantName, c.Subscriptions[0].TenantName);
                //Assert.Equal(s1.Status, c.Subscriptions[0].Status);
                
                //Assert.Single(c.Users);
                //Assert.Equal(u1.Id, c.Users[0].Id);
                //Assert.Equal(u1.CompanyId, c.Users[0].CompanyId);
                //Assert.Equal(u1.Role, c.Users[0].Role);
                //Assert.Equal(u1.FirstName, c.Users[0].FirstName);
                //Assert.Equal(u1.Surname, c.Users[0].Surname);
                //Assert.Equal(u1.Email, c.Users[0].Email);
                //Assert.Equal(u1.Phone, c.Users[0].Phone);
                //Assert.Equal(u1.Notes, c.Users[0].Notes);
                //Assert.Equal(u1.Status, c.Users[0].Status);
            });
        }
        
        private async void ExecuteTestCase(Func<GenericRepository<Company>, Task> action)
        {
            var options = new DbContextOptionsBuilder<DefaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new DefaultDbContext(options))
            {
                // It's VERY important.
                await context.Database.EnsureCreatedAsync();

                var repository = new GenericRepository<Company>(context);

                await action(repository);
            }
        }
    }
}
