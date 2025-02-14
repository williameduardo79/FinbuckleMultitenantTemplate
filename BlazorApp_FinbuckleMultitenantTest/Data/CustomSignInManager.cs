using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomSignInManager(
         UserManager<ApplicationUser> userManager,
         IHttpContextAccessor httpContextAccessor,
         IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
         IOptions<IdentityOptions> optionsAccessor,
         ILogger<SignInManager<ApplicationUser>> logger,
         IAuthenticationSchemeProvider authenticationSchemeProvider,
         IUserConfirmation<ApplicationUser> userConfirmation, // Add this
         IMultiTenantContextAccessor multiTenantContextAccessor)
         : base(userManager, httpContextAccessor, claimsFactory, optionsAccessor, logger, authenticationSchemeProvider, userConfirmation)
        {
            _multiTenantContextAccessor = multiTenantContextAccessor;  // Store the multi-tenant context
        }

        // Override sign-in method
        public override async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // Ensure that the correct tenant context is set when signing in
            var currentTenant = _multiTenantContextAccessor?.MultiTenantContext.TenantInfo;
            if (currentTenant == null)
            {
                // Handle the case where the tenant context is not set
                return SignInResult.Failed;
            }

            // Check user’s tenant association
            var userTenants = user.UserTenants.Where(ut => ut.TenantId == currentTenant.Id).ToList();
            if (userTenants.Count == 0)
            {
                // Handle case where user is not part of the current tenant
                return SignInResult.Failed;
            }

            // Proceed with sign-in
            return await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
       
    }

}
