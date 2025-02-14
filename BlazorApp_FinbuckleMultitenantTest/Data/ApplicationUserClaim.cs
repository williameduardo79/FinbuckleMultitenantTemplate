using Microsoft.AspNetCore.Identity;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public string TenantId { get; set; } = string.Empty; // Ensuring it's not null
    }
}
