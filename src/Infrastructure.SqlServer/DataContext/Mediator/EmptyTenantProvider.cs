using PingDong.CleanArchitect.Service;

namespace PingDong.CleanArchitect.Infrastructure.SqlServer
{
    public partial class GenericDbContext
    {
        public class EmptyTenantProvider<T> : ITenantProvider<T>
        {
            public T GetTenantId()
            {
                return default;
            }
        }
    }
}
