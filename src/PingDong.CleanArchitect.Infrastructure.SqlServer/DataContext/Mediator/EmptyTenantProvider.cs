using System;
using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public partial class GenericDbContext
    {
        public class EmptyTenantProvider : ITenantProvider
        {
            public Guid GetTenantId()
            {
                return Guid.NewGuid();
            }
        }
    }
}
