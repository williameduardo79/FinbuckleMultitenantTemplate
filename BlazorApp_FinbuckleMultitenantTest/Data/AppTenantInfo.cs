using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class AppTenantInfo : ITenantInfo
    {
        public string Id { get; set; } = default!;
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ConnectionString { get; set; } = default!;
        public string TenantAddress { get; set; } = default!;
        public bool IsMainTenant { get; set; } = false;
       
    }
}
