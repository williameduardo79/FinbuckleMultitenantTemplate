using BlazorApp_FinbuckleMultitenantTest.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserTenant
{

    public string UserId { get; set; } = default!;
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = default!;

    public string TenantId { get; set; } = default!;
    [ForeignKey("TenantId")]
    public virtual AppTenantInfo Tenant { get; set; } = default!;
}
