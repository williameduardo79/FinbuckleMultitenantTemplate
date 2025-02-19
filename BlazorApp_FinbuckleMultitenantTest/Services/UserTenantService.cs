using BlazorApp_FinbuckleMultitenantTest.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public class UserTenantService : IUserTenantService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserTenantService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get a single UserTenant by composite key
        public async Task<UserTenant?> GetUserTenantAsync(string userId, string tenantId)
        {
            return await _dbContext.UserTenants
                .Include(ut => ut.User)
                .Include(ut => ut.Tenant)
                
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
        }

        // Get all UserTenants
        public async Task<List<UserTenant>> GetAllUserTenantsAsync()
        {
            return await _dbContext.UserTenants
                .Include(ut => ut.User)
                .Include(ut => ut.Tenant)
               
                .ToListAsync();
        }

        // Create a new UserTenant
        public async Task<UserTenant> CreateUserTenantAsync(UserTenant userTenant)
        {
            _dbContext.UserTenants.Add(userTenant);
            await _dbContext.SaveChangesAsync();
            return userTenant;
        }

        // Update an existing UserTenant
        public async Task<UserTenant> UpdateUserTenantAsync(UserTenant userTenant)
        {
            var existingUserTenant = await _dbContext.UserTenants
                .FirstOrDefaultAsync(ut => ut.UserId == userTenant.UserId && ut.TenantId == userTenant.TenantId);

            if (existingUserTenant == null)
            {
                throw new InvalidOperationException("User-Tenant not found.");
            }

            _dbContext.Entry(existingUserTenant).CurrentValues.SetValues(userTenant);
            await _dbContext.SaveChangesAsync();
            return userTenant;
        }

        // Delete a UserTenant by composite key
        public async Task<bool> DeleteUserTenantAsync(string userId, string tenantId)
        {
            var userTenant = await _dbContext.UserTenants
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);

            if (userTenant == null)
            {
                return false;
            }

            _dbContext.UserTenants.Remove(userTenant);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
