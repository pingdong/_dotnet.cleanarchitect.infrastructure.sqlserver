using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public class ClientRequestEntityTypeConfiguration<T> : IEntityTypeConfiguration<ClientRequest<T>>
    {
        private readonly string _schema;
        private readonly string _tableName;

        public ClientRequestEntityTypeConfiguration(string tableName = "Requests", string schema = null)
        {
            _schema = schema;
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<ClientRequest<T>> configuration)
        {
            configuration.ToTable(_tableName, _schema);

            configuration.HasKey(cr => cr.Id);
            configuration.Property(cr => cr.Name).IsRequired();
            configuration.Property(cr => cr.Time).IsRequired();
            configuration.Ignore(cr => cr.TenantId);
            configuration.Ignore(cr => cr.CorrelationId);
            configuration.Ignore(cr => cr.DomainEvents);
        }
    }
}
