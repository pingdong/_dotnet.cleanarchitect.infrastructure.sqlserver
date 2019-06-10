using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency
{
    internal class ClientRequestEntityTypeConfiguration<T> : IEntityTypeConfiguration<ClientRequest<T>>
    {
        private readonly string _defaultSchema;
        public ClientRequestEntityTypeConfiguration(string defaultSchema)
        {
            _defaultSchema = defaultSchema;
        }

        public void Configure(EntityTypeBuilder<ClientRequest<T>> requestConfiguration)
        {
            requestConfiguration.ToTable("RequestsManager", _defaultSchema);

            requestConfiguration.HasKey(cr => cr.Id);
            requestConfiguration.Property(cr => cr.Name).IsRequired();
            requestConfiguration.Property(cr => cr.Time).IsRequired();
        }
    }
}
