using BlazorApp_FinbuckleMultitenantTest.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public class UserTenantRoleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<TenantRole> _roleManager;

        public UserTenantRoleService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<TenantRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Assign Role to User for a Specific Tenant
        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName, string tenantId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
            if (role == null)
            {
                // Create role if it doesn't exist
                role = new TenantRole { Name = roleName, TenantId = tenantId };
                await _roleManager.CreateAsync(role);
            }

            // Check if the User already has the role
            var userHasRole = await _dbContext.UserTenantRoles
                .AnyAsync(utr => utr.UserId == userId && utr.RoleId == role.Id && utr.TenantId == tenantId);
            if (userHasRole) return false;

            // Assign role to user for this tenant
            var userTenantRole = new UserTenantRole
            {
                UserId = userId,
                RoleId = role.Id,
                TenantId = tenantId
            };
            _dbContext.UserTenantRoles.Add(userTenantRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Get User Roles for a Tenant
        public async Task<List<string>> GetUserRolesAsync(string userId, string tenantId)
        {
            return await _dbContext.UserTenantRoles
                .Where(utr => utr.UserId == userId && utr.TenantId == tenantId)
                .Select(utr => utr.Role.Name)
                .ToListAsync();
        }

        // Remove Role from User for a Specific Tenant
        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName, string tenantId)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
            if (role == null) return false;

            var userTenantRole = await _dbContext.UserTenantRoles
                .FirstOrDefaultAsync(utr => utr.UserId == userId && utr.RoleId == role.Id && utr.TenantId == tenantId);
            if (userTenantRole == null) return false;

            _dbContext.UserTenantRoles.Remove(userTenantRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
