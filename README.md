# FinbuckleMultitenantTemplate
 'dotnet ef migrations add InitialConfig -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext

dotnet ef migrations add InitialCreate -c TenantDbContext
dotnet ef database update -c TenantDbContext
'
