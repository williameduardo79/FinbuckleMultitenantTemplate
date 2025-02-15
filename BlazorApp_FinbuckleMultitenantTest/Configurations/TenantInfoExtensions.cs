using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public static class TenantInfoExtensions
    {
        public static string? GetConnectionString(this ITenantInfo tenantInfo)
        {
            return (tenantInfo as AppTenantInfo)?.ConnectionString;
        }
    }

}
