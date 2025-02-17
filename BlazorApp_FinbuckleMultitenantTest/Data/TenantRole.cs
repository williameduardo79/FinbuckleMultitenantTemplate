using Microsoft.AspNetCore.Identity;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class TenantRole : IdentityRole
    {
        public string TenantId { get; set; } = default!;
        // Default constructor required by EF Core
        public TenantRole() : base() { }

        // Constructor that accepts a role name
        public TenantRole(string roleName, string tenantId) : base(roleName)
        {
            TenantId = tenantId;
        }


    }
}
