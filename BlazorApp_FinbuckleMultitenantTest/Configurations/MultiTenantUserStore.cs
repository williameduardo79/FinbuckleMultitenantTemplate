using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.Abstractions;
using BlazorApp_FinbuckleMultitenantTest.Data;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public class MultiTenantUserStore : UserStore<ApplicationUser, TenantRole, ApplicationDbContext, string>
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
        public override async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>();
            if (tenantInfo == null) throw new InvalidOperationException("Tenant info is null");

            // Query only roles for the current tenant
            var roles = await Context.UserTenantRoles
                .Where(utr => utr.UserId == user.Id && utr.TenantId == tenantInfo.Id)
                .Select(utr => utr.Role.Name)
                .ToListAsync(cancellationToken);

            return roles;
        }
        public override async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var tenantInfo = _httpContextAccessor.HttpContext?.GetTenantInfo<AppTenantInfo>();
            if (tenantInfo == null) throw new InvalidOperationException("Tenant info is null");

            var role = await Context.TenantRoles
                .Where(r => r.Name == roleName && r.TenantId == tenantInfo.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (role == null) throw new InvalidOperationException($"Role '{roleName}' not found for tenant '{tenantInfo.Id}'");

            var userRole = new UserTenantRole
            {
                UserId = user.Id,
                TenantId = tenantInfo.Id,
                RoleId = role.Id
            };

            Context.UserTenantRoles.Add(userRole);
            await Context.SaveChangesAsync(cancellationToken);
        }

    }
}
