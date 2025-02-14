using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext<ApplicationUser, IdentityRole, string,
    ApplicationUserClaim, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions<ApplicationDbContext> options)
            : base(multiTenantContextAccessor, options)
        {
        }
        public DbSet<AppTenantInfo> Tenants { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<ApplicationUserClaim> UserClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<UserTenant>()
                .HasKey(ut => new { ut.UserId, ut.TenantId });

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(ut => ut.TenantId);

            //Ensure TenantId is required in the claims table
            modelBuilder.Entity<ApplicationUserClaim>()
                .Property(uc => uc.TenantId)
                .IsRequired();

            // Index for faster multi-tenant lookups
            modelBuilder.Entity<ApplicationUserClaim>()
                .HasIndex(uc => new { uc.UserId, uc.TenantId });

            // Override IdentityUserClaim with ApplicationUserClaim
            modelBuilder.Entity<ApplicationUserClaim>()
                .ToTable("AspNetUserClaims");
        }
    }

}
