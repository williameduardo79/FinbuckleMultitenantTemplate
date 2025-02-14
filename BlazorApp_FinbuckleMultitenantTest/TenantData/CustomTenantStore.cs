using Finbuckle.MultiTenant;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.TenantData
{
    public class CustomTenantStore : IMultiTenantStore<AppTenantInfo>
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomTenantStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Try to add the TTenantInfo to the store.
        /// </summary>
        public async Task<bool> TryAddAsync(AppTenantInfo tenantInfo)
        {
            if (tenantInfo == null)
                return false;

            _dbContext.Tenants.Add(tenantInfo);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0; // Return true if at least one row was affected
        }

        /// <summary>
        /// Try to update the TTenantInfo in the store.
        /// </summary>
        public async Task<bool> TryUpdateAsync(AppTenantInfo tenantInfo)
        {
            if (tenantInfo == null)
                return false;

            _dbContext.Tenants.Update(tenantInfo);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0; // Return true if at least one row was affected
        }

        /// <summary>
        /// Try to remove the TTenantInfo from the store.
        /// </summary>
        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var tenantInfo = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == identifier);

            if (tenantInfo == null)
                return false;

            _dbContext.Tenants.Remove(tenantInfo);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0; // Return true if at least one row was affected
        }

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        [Obsolete("Use TryGetByIdentifierAsync instead.")]
        public async Task<AppTenantInfo?> TryGetAsync(string id)
        {
            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        public async Task<AppTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == identifier);
        }

        /// <summary>
        /// Retrieve all the TTenantInfo's from the store.
        /// </summary>
        public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
        {
            return await _dbContext.Tenants
                .ToListAsync();
        }
    }

}
