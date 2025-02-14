using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp_FinbuckleMultitenantTest.TenantData
{
    using Microsoft.EntityFrameworkCore;
    using Finbuckle.MultiTenant.EntityFrameworkCore;

    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }
        // Explicitly add a DbSet for AppTenantInfo
        public DbSet<AppTenantInfo> Tenants { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure AppTenantInfo is configured properly
            builder.Entity<AppTenantInfo>().ToTable("Tenants");
        }
    }


}
