using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    using Finbuckle.MultiTenant; // Make sure you have this using statement

    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, TenantRole>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMultiTenantContextAccessor _tenantAccessor;
        private readonly TenantUserManager _userManager;

        public CustomUserClaimsPrincipalFactory(
     TenantUserManager userManager,
     RoleManager<TenantRole> roleManager,
     IOptions<IdentityOptions> optionsAccessor,
     IHttpContextAccessor httpContextAccessor,
     IMultiTenantContextAccessor tenantAccessor)
     : base(userManager,roleManager, optionsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantAccessor = tenantAccessor;
            _userManager = userManager; // Assign the user manager properly
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
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (tenantId != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return identity;
        }
    }
}
