using BlazorApp_FinbuckleMultitenantTest.Data;

namespace BlazorApp_FinbuckleMultitenantTest.Models
{
    public class UserTenantRoleDTO
    {
        public string UserId { get; set; }  // u.Id
        public ApplicationUser user { get; set; }
        public List<TenantRoleDTO>?  TenantRoleDTOs { get; set; }

    }

}
