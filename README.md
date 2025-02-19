# Finbuckle Multitenant Template
## Run the following commands to add and update the migrations for the `ApplicationDbContext`:

```
dotnet ef migrations add InitialConfig -c ApplicationDbContext

dotnet ef database update -c ApplicationDbContext
```

## Then, add and update the migrations for the TenantDbContext, you may update the tenant connection string multiple times to create new instances as multiple tenants:

```
dotnet ef migrations add InitialCreate -c TenantDbContext

dotnet ef database update -c TenantDbContext
```


