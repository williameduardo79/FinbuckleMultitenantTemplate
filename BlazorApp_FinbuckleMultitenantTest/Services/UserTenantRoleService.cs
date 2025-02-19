using BlazorApp_FinbuckleMultitenantTest.Configurations;
using BlazorApp_FinbuckleMultitenantTest.Data;
using BlazorApp_FinbuckleMultitenantTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public class UserTenantRoleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TenantUserManager _userManager;
        private readonly RoleManager<TenantRole> _roleManager;

        public UserTenantRoleService(ApplicationDbContext dbContext, TenantUserManager userManager, RoleManager<TenantRole> roleManager)
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
        public async Task<List<UserTenantRoleDTO>> GetAllUserTenantsWithRoles()
        {
            var userTenantRoles = await (
                from u in _dbContext.Users
                join ut in _dbContext.UserTenants on u.Id equals ut.UserId into userTenants
                from ut in userTenants.DefaultIfEmpty() // Left join to include users without tenants
                join t in _dbContext.Tenants on ut.TenantId equals t.Id into tenants
                from t in tenants.DefaultIfEmpty() // Left join to include users without tenants
                join ur in _dbContext.UserTenantRoles on new { ut.UserId, ut.TenantId } equals new { ur.UserId, ur.TenantId } into userRoles
                from ur in userRoles.DefaultIfEmpty() // Left join to include users without roles
                join r in _dbContext.Roles on ur.RoleId equals r.Id into roles
                from r in roles.DefaultIfEmpty() // Left join to include tenants without roles
                select new
                {
                    User = u,
                    Tenant = ut,
                    TenantInfo = t,
                    Role = r
                }
            ).ToListAsync();

            var groupedUsers = userTenantRoles
                .GroupBy(ur => ur.User.Id)
                .Select(g => new UserTenantRoleDTO
                {
                    UserId = g.Key,
                    user = g.First().User, // Get the user object
                    TenantRoleDTOs = g.GroupBy(ut => ut.Tenant?.TenantId) // Group by TenantId
                        .Where(ut => ut.Key != null) // Ignore users without tenants
                        .Select(ut => new TenantRoleDTO
                        {
                            TenantId = ut.Key!,
                            UserWithTenant = ut.First().Tenant!, // User-Tenant relationship
                            UserTenantRoles = ut
                                .Where(utr => utr.Role != null) // Ignore if no roles
                                .Select(utr => new UserTenantRole
                                {
                                    UserId = utr.User.Id,
                                    TenantId = utr.Tenant!.TenantId,
                                    Role = utr.Role!
                                }).ToList()
                        }).ToList()
                }).ToList();

            return groupedUsers;
        }


    }
}
