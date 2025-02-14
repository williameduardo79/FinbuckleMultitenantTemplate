using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class MultiTenantUserStore : UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MultiTenantUserStore(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IdentityErrorDescriber? describer = null)
       : base(context, describer)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public override async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>();
            if (tenantInfo == null) throw new InvalidOperationException("Tenant info is null");

            // Ensure TenantId is included in query
            var claims = await Context.UserClaims
                .Where(uc => uc.UserId == user.Id && uc.TenantId == tenantInfo.Id)
                .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue))
                .ToListAsync(cancellationToken);

            return claims;
        }

        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>();
            if (tenantInfo != null)
            {
                user.UserTenants.Add(new UserTenant
                {
                    UserId = user.Id,
                    TenantId = tenantInfo.Id
                });
            }

            var result = await base.CreateAsync(user, cancellationToken);
            return result;
        }
        public override async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>();
            if (tenantInfo == null) throw new InvalidOperationException("Tenant info is null");

            foreach (var claim in claims)
            {
                var userClaim = new ApplicationUserClaim
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    TenantId = tenantInfo.Id // Assign the tenant ID
                };

                Context.UserClaims.Add(userClaim);
            }

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
