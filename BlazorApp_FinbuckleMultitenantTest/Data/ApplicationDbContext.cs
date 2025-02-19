using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext<ApplicationUser, TenantRole, string,
    IdentityUserClaim<string>, UserTenantRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions<ApplicationDbContext> options)
            : base(multiTenantContextAccessor, options)
        {
        }
        public DbSet<AppTenantInfo> Tenants { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<ApplicationUserClaim> UserClaims { get; set; }
        public DbSet<UserTenantRole> UserTenantRoles { get; set; }

        public DbSet<TenantRole> TenantRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantRole>()
          .HasIndex(r => new { r.TenantId, r.Name })
          .IsUnique();

            // Configure AppTenantInfo - Index for IsMainTenant field
            modelBuilder.Entity<AppTenantInfo>()
                .HasIndex(t => t.IsMainTenant)
                .IsUnique()
                .HasFilter("[IsMainTenant] = 1"); // Modify as needed for your DB provider

            // Configure relationships for UserTenantRole (User -> Tenant -> Role)
            modelBuilder.Entity<UserTenantRole>()
                .HasKey(utr => new { utr.UserId, utr.TenantId, utr.RoleId });

            modelBuilder.Entity<UserTenantRole>()
                .HasOne(utr => utr.User)
                .WithMany()
                .HasForeignKey(utr => utr.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            modelBuilder.Entity<UserTenantRole>()
                .HasOne(utr => utr.Role)
                .WithMany()
                .HasForeignKey(utr => utr.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Ensure TenantId is included in the UserTenantRole relation
            modelBuilder.Entity<UserTenantRole>()
                .HasOne(utr => utr.Tenant)
                .WithMany()
                .HasForeignKey(utr => utr.TenantId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

           

            // Configure UserTenantRole to include TenantRole (Role -> Tenant)
            modelBuilder.Entity<TenantRole>()
                .HasIndex(r => new { r.TenantId, r.Name })
                .IsUnique(); // Ensure roles are unique per tenant

            // Explicitly map the ApplicationUserClaim entity
            modelBuilder.Entity<ApplicationUserClaim>()
                .ToTable("IdentityUserClaims");

            modelBuilder.Entity<UserTenant>()
       .HasKey(ut => new { ut.UserId, ut.TenantId });

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTenant>()
                .HasOne(ut => ut.Tenant)
                .WithMany()
                .HasForeignKey(ut => ut.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
