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
      
        public DbSet<Product> Products { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
          .Property(p => p.Id)
          .ValueGeneratedOnAdd();  // Ensures auto-increment behavior for `Id`

        }
    }


}
