using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class InfrastructureRegistrar
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">IConfiguration</param>
        /// <param name="requestTableSchema">Schema for RequestsManager table, default is dbo</param>
        public InfrastructureRegistrar(IConfiguration config, string requestTableSchema = "dbo")
        {
            _configuration = config;

            RequestsManagerTable.DefaultSchema = requestTableSchema;
        }
        
        /// <summary>
        /// GenericDbContext retrieve "Default" from Section "ConnectionStrings" from IConfiguration.
        /// If GenericDbContext has to use another value, need override its key.
        /// </summary>
        public string ConnectionStringKey { get; set; } = "Default";

        /// <summary>
        /// If user need full control the connection or even use a database other than mssqlserver,
        /// user has to set options by override the method
        /// </summary>
        /// <param name="options">DbContextOptionsBuilder</param>
        public virtual void BuildDbContext(DbContextOptionsBuilder options)
        {
            var builder = options.UseSqlServer(_configuration.GetConnectionString(ConnectionStringKey),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });
        }
        
        /// <summary>
        /// Determine whether uses the default GenericRepository. 
        /// If user needs to have its own the implementation of IRepository, it needs to register your own implementation
        /// </summary>
        public virtual void RegisterGenericRepository(IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
        }

        public virtual void Register(IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<GenericDbContext>(BuildDbContext);

            // Register all repositories
            RegisterGenericRepository(services);
        }
    }
}
