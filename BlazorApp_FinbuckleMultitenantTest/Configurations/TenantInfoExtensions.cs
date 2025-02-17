using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public static class TenantInfoExtensions
    {
        public static string? GetConnectionString(this ITenantInfo tenantInfo)
        {
            return (tenantInfo as AppTenantInfo)?.ConnectionString;
        }
    }

}
