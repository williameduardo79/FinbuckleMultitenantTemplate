using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    using Finbuckle.MultiTenant; // Make sure you have this using statement

    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor, IHttpContextAccessor httpContextAccessor)
            : base(userManager, optionsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            // Access ITenantInfo directly from the HttpContext
            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>(); // Replace AppTenantInfo with your tenant info type

            if (tenantInfo != null)
            {
                identity.AddClaim(new Claim("TenantId", tenantInfo.Id));
                identity.AddClaim(new Claim("TenantName", tenantInfo.Name));
                // ... add other tenant-specific claims
            }
            else
            {
                // Handle the case where tenantInfo is null (e.g., log a warning)
                Console.WriteLine("Tenant information not found in HttpContext.");
            }

            return principal;
        }
    }
}
