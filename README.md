# Finbuckle Multitenant Template
##Run the following commands to add and update the migrations for the `ApplicationDbContext`:
 `dotnet ef migrations add InitialConfig -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext`
##Then, add and update the migrations for the TenantDbContext:
`
dotnet ef migrations add InitialCreate -c TenantDbContext
dotnet ef database update -c TenantDbContext
`
