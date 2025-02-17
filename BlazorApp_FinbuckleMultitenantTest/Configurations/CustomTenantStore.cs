using Finbuckle.MultiTenant;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public class CustomTenantStore : IMultiTenantStore<AppTenantInfo>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey = "CachedTenants"; // Key for storing tenants in memory
        public CustomTenantStore(ApplicationDbContext dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
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

            if (result > 0)
                _cache.Remove(_cacheKey); // Invalidate cache after adding a tenant

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

            if (result > 0)
                _cache.Remove(_cacheKey); // Invalidate cache after updating a tenant

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

            if (result > 0)
                _cache.Remove(_cacheKey); // Invalidate cache after updating a tenant

            return result > 0; // Return true if at least one row was affected
        }

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        [Obsolete("Use TryGetByIdentifierAsync instead.")]
        public async Task<AppTenantInfo?> TryGetAsync(string id)
        {
            var tenants = await GetTenantsFromCache(); // Await the Task to get IQueryable
            return tenants.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        public async Task<AppTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            var tenants = await GetTenantsFromCache(); // Await the Task to get IQueryable
            return tenants.FirstOrDefault(t => t.Identifier == identifier);
        }

        /// <summary>
        /// Retrieve all the TTenantInfo's from the store.
        /// </summary>
        public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
        {
            var tenants = await GetTenantsFromCache();
            return tenants;
        }
        /// <summary>
        /// Retrieves tenants from cache or database if not cached.
        /// </summary>
        private async Task<List<AppTenantInfo>> GetTenantsFromCache()
        {
            if (!_cache.TryGetValue(_cacheKey, out List<AppTenantInfo>? tenants))
            {
                tenants = await _dbContext.Tenants.ToListAsync();

                // Cache the tenants with a 10-minute expiration
                _cache.Set(_cacheKey, tenants, TimeSpan.FromMinutes(10));
            }

            return tenants;
        }
    }

}
