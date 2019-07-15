using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency
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

        public void Configure(EntityTypeBuilder<ClientRequest<T>> requestConfiguration)
        {
            requestConfiguration.ToTable(_tableName, _schema);

            requestConfiguration.HasKey(cr => cr.Id);
            requestConfiguration.Property(cr => cr.Name).IsRequired();
            requestConfiguration.Property(cr => cr.Time).IsRequired();
        }
    }
}
