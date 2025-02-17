using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public class TenantUserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMultiTenantContextAccessor _tenantAccessor;

        public TenantUserManager(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger,
            ApplicationDbContext context,
            IMultiTenantContextAccessor tenantAccessor
        ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
        }

        public override async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (tenantId == null) throw new InvalidOperationException("Tenant ID is required.");

            return await _context.UserTenantRoles.AnyAsync(utr =>
                utr.UserId == user.Id &&
                utr.TenantId == tenantId &&
                utr.Role.Name == roleName);
        }
        public override async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (tenantId == null) throw new InvalidOperationException("Tenant ID is required.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} not found." });

            if (await IsInRoleAsync(user, roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = $"User is already in role {roleName} for tenant {tenantId}." });
            }

            _context.UserTenantRoles.Add(new UserTenantRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                TenantId = tenantId
            });

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        public override async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (tenantId == null) throw new InvalidOperationException("Tenant ID is required.");

            return await _context.UserTenantRoles
                .Where(utr => utr.UserId == user.Id && utr.TenantId == tenantId)
                .Select(utr => utr.Role.Name)
                .ToListAsync();
        }

    }

}
