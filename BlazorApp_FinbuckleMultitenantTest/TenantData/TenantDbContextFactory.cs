using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp_FinbuckleMultitenantTest.TenantData
{
    public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            // Load the configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            // Get the connection string for the tenant database
            string tenantConnectionString = config.GetConnectionString("TenantConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=aspnet-TenantDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            optionsBuilder.UseSqlServer(tenantConnectionString);

            return new TenantDbContext(optionsBuilder.Options);
        }
    }
}
