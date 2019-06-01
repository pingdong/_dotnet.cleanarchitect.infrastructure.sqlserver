using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer.Idempotency
{
    internal class ClientRequestEntityTypeConfiguration : IEntityTypeConfiguration<ClientRequest>
    {
        private readonly string _defaultSchema;
        public ClientRequestEntityTypeConfiguration(string defaultSchema)
        {
            _defaultSchema = defaultSchema;
        }

        public void Configure(EntityTypeBuilder<ClientRequest> requestConfiguration)
        {
            requestConfiguration.ToTable("RequestsManager", _defaultSchema);

            requestConfiguration.HasKey(cr => cr.Id);
            requestConfiguration.Property(cr => cr.Name).IsRequired();
            requestConfiguration.Property(cr => cr.Time).IsRequired();
        }
    }
}
