using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class UserTenantRole : IdentityUserRole<string>
    {
        public string TenantId { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = default!;

        [ForeignKey("RoleId")]
        public virtual TenantRole Role { get; set; } = default!;
        [ForeignKey("TenantId")]
        public virtual AppTenantInfo Tenant { get; set; } = default!;
    }
}
