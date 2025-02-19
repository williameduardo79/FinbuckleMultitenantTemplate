using BlazorApp_FinbuckleMultitenantTest.Data;

namespace BlazorApp_FinbuckleMultitenantTest.Models
{
    public class TenantRoleDTO
    {
        public string TenantId { get; set; }
        public UserTenant UserWithTenant { get; set; }
        public List<UserTenantRole>? UserTenantRoles { get; set; }
    }

}
