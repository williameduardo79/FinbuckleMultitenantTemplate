using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.Abstractions;
using BlazorApp_FinbuckleMultitenantTest.Data;
using System.Threading;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public class MultiTenantUserStore : UserStore<ApplicationUser, TenantRole, ApplicationDbContext, string>
    {

        public MultiTenantUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
       : base(context, describer)
        {
           
           
        }
       
        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant info is null");

            // Ensure TenantId is included in query
            var claims = await Context.UserClaims
                .Where(uc => uc.UserId == user.Id && uc.TenantId == tenantId)
                .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue))
                .ToListAsync(cancellationToken);

            return claims;
        }
       
       
       public async Task CreateUserTenantAsync(UserTenant userTenant, CancellationToken cancellationToken)
        {

            await Context.UserTenants.AddAsync(userTenant, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }
       
        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, string tenantId, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant info is null");

            foreach (var claim in claims)
            {
                var userClaim = new ApplicationUserClaim
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    TenantId = tenantId // Assign the tenant ID
                };

                Context.UserClaims.Add(userClaim);
            }

            await Context.SaveChangesAsync(cancellationToken);
        }
     
        public async Task<IList<string>> GetRolesAsync(ApplicationUser user,string tenantId, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant info is null");

            // Query only roles for the current tenant
            var roles = await Context.UserTenantRoles
                .Where(utr => utr.UserId == user.Id && utr.TenantId == tenantId)
                .Select(utr => utr.Role.Name)
                .ToListAsync(cancellationToken);

            return roles;
        }
        public async Task<IList<UserTenantRole>> GetAllUserRoles()
        {
            return await Context.UserTenantRoles.ToListAsync();
        }
        public async Task AddToRoleAsync(ApplicationUser user, string roleName,string tenantId, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant info is null");

            var role = await Context.TenantRoles
                .Where(r => r.Name == roleName && r.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (role == null) throw new InvalidOperationException($"Role '{roleName}' not found for tenant '{tenantId}'");

            var userRole = new UserTenantRole
            {
                UserId = user.Id,
                TenantId = tenantId,
                RoleId = role.Id
            };

            Context.UserTenantRoles.Add(userRole);
            await Context.SaveChangesAsync(cancellationToken);
        }
        public async Task<bool> IsInRoleAsync(string userId,string tenantId,string roleName)
        {
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant info is null");
            if (string.IsNullOrEmpty(userId)) throw new InvalidOperationException("User Id is null");
            if (string.IsNullOrEmpty(roleName)) throw new InvalidOperationException("Role Name is null");
            return await Context.UserTenantRoles.AnyAsync(utr =>
               utr.UserId == userId &&
               utr.TenantId == tenantId &&
               utr.Role.Name == roleName);
        }

    }
}
