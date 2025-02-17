using BlazorApp_FinbuckleMultitenantTest.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public class AppTenantInfoService : IAppTenantInfoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppTenantInfoService> _logger;

        public AppTenantInfoService(ApplicationDbContext context, ILogger<AppTenantInfoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all tenants.
        /// </summary>
        public async Task<List<AppTenantInfo>> GetAllAsync()
        {
            return await _context.Tenants
                .Include(t => t.UserTenants)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a tenant by its ID.
        /// </summary>
        public async Task<AppTenantInfo?> GetByIdAsync(string id)
        {
            return await _context.Tenants
                .Include(t => t.UserTenants)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Adds a new tenant.
        /// </summary>
        public async Task<AppTenantInfo> CreateAsync(AppTenantInfo tenant)
        {
            if (tenant.IsMainTenant)
            {
                await UnsetExistingMainTenantAsync();
            }

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        public async Task<bool> UpdateAsync(AppTenantInfo tenant)
        {
            var existingTenant = await _context.Tenants.FindAsync(tenant.Id);
            if (existingTenant == null)
            {
                _logger.LogWarning($"Tenant with ID {tenant.Id} not found.");
                return false;
            }

            if (tenant.IsMainTenant && !existingTenant.IsMainTenant)
            {
                await UnsetExistingMainTenantAsync();
            }

            existingTenant.Identifier = tenant.Identifier;
            existingTenant.Name = tenant.Name;
            existingTenant.ConnectionString = tenant.ConnectionString;
            existingTenant.TenantAddress = tenant.TenantAddress;
            existingTenant.IsMainTenant = tenant.IsMainTenant;

            _context.Tenants.Update(existingTenant);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a tenant by ID.
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                _logger.LogWarning($"Tenant with ID {id} not found.");
                return false;
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            return true;
        }
        private async Task UnsetExistingMainTenantAsync()
        {
            var currentMainTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.IsMainTenant);

            if (currentMainTenant != null)
            {
                currentMainTenant.IsMainTenant = false;
                _context.Tenants.Update(currentMainTenant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
