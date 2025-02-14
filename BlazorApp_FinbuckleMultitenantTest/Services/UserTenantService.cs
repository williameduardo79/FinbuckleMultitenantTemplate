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

        // Get a single UserTenant by Id
        public async Task<UserTenant> GetUserTenantAsync(int id)
        {
            return await _dbContext.UserTenants
                .Include(ut => ut.User)
                .Include(ut => ut.Tenant)
                .FirstOrDefaultAsync(ut => ut.Id == id);
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
            _dbContext.UserTenants.Update(userTenant);
            await _dbContext.SaveChangesAsync();
            return userTenant;
        }

        // Delete a UserTenant by Id
        public async Task<bool> DeleteUserTenantAsync(int id)
        {
            var userTenant = await _dbContext.UserTenants.FindAsync(id);
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
