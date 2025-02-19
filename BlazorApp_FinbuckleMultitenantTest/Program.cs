using BlazorApp_FinbuckleMultitenantTest.Components;
using BlazorApp_FinbuckleMultitenantTest.Components.Account;
using BlazorApp_FinbuckleMultitenantTest.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using BlazorApp_FinbuckleMultitenantTest.TenantData;
using Finbuckle.MultiTenant.Abstractions;
using System.Security.Principal;
using BlazorApp_FinbuckleMultitenantTest.Services;
using BlazorApp_FinbuckleMultitenantTest.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using BlazorApp_FinbuckleMultitenantTest.Configurations;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache(); // Register MemoryCache

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Add authentication services before AddIdentity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;  // This ensures the correct scheme is used
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme; // Ensure the same for sign-in
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.Identity";  // Set the cookie name here
    options.Cookie.SameSite = SameSiteMode.Lax;
});


// Now, configure the ApplicationDbContext for your main application
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    // Other Identity options
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
})
    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
    .AddRoles<TenantRole>() // 🔹 Add this to specify role type
    .AddEntityFrameworkStores<ApplicationDbContext>() //Ensures EF Core stores roles
    .AddSignInManager<SignInManager<ApplicationUser>>() //Adds SignInManager
    .AddUserManager<TenantUserManager>()
    .AddRoleManager<RoleManager<TenantRole>>() // Role Manager for Tenant Roles
    .AddUserStore<MultiTenantUserStore>()
    .AddDefaultTokenProviders(); //Ensures token support (e.g., for password reset)


//builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();
builder.Services.AddScoped<MultiTenantUserStore>();
builder.Services.AddScoped<TenantUserManager>(); // Register TenantUserManager as Scoped




builder.Services.AddHttpContextAccessor();

// Add the TenantDbContext for tenant-specific data access
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TenantConnection")));

//Finbuckle Multitenant
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithStore<CustomTenantStore>(ServiceLifetime.Scoped)
    .WithHostStrategy();


builder.Services.AddScoped<IUserTenantService, UserTenantService>();
//views
builder.Services.AddScoped<UserTenantRoleService>(); // Register UserTenantRoleService as Scoped
//builder.Services.AddScoped<IAppTenantInfoService, AppTenantInfoService>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add session services to the DI container
builder.Services.AddDistributedMemoryCache(); // Optional: You can use a distributed cache, or use in-memory cache.
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // To make the session cookie accessible only to the server
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Optional: Set session timeout
});

var app = builder.Build();

app.UseMultiTenant();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseAuthentication();  // Authentication must come before Authorization
app.UseAuthorization();   // Authorization must follow authentication

// Session should come after Authentication
app.UseSession();         // This should come after UseAuthentication

// Antiforgery should be configured after session and before other middleware
app.UseAntiforgery();     // Configure antiforgery cookies and tokens

app.UseStaticFiles();
app.UseMiddleware<TenantCookieMiddleware>();    // Custom tenant cookie logic
app.UseMiddleware<AntiforgeryMiddleware>();    // Custom antiforgery middleware

// After all the middlewares, map the endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = services.GetRequiredService<IConfiguration>();
    await CreateRolesAsync(services, configuration);
}


app.Run();

static async Task CreateRolesAsync(IServiceProvider serviceProvider, IConfiguration configuration)
{
    var mainTenantAdmin = ApplicationVariables.MainTenantsRole;
    var mainTenantId = configuration["AdminSettings:MainTenantId"];
    // Retrieve required services
    var multiTenantStore = serviceProvider.GetRequiredService<IMultiTenantStore<AppTenantInfo>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<TenantRole>>();
    var userManager = serviceProvider.GetRequiredService<TenantUserManager>();
    var userTenantService = serviceProvider.GetRequiredService<IUserTenantService>();

    // Step 1: Ensure the Main Tenant Exists
    var mainTenant = (await multiTenantStore.GetAllAsync()).SingleOrDefault(x => x.IsMainTenant);
   
    if (mainTenant == null || mainTenantId == null)
    {
        mainTenant = new AppTenantInfo
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = mainTenantId,
            Name = "Main Tenant",
            TenantAddress = mainTenantId,
            ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=aspnet-BlazorApp_FinbuckleMultitenantTest-MainTenant;Trusted_Connection=True;MultipleActiveResultSets=true",
            IsMainTenant = true
        };

        await multiTenantStore.TryAddAsync(mainTenant);
    }

    // Step 2: Create Roles BEFORE Assigning Users
    var roleNames = new[] { mainTenantAdmin, "Admin", "SuperUser" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new TenantRole(roleName, mainTenant.Id));
            if (!result.Succeeded)
            {
                Console.WriteLine($"Error creating role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    // Step 3: Admin User Creation
    var adminEmail = configuration["AdminSettings:UserEmail"];
    var adminUserName = configuration["AdminSettings:UserName"];
    var adminPassword = configuration["AdminSettings:UserPassword"];

    if (!string.IsNullOrWhiteSpace(adminUserName) && !string.IsNullOrWhiteSpace(adminEmail))
    {
        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        ApplicationUser adminUser;

        if (existingUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail
                
            };
     
            var createUser = await userManager.CreateWithTenantAsync(adminUser, adminPassword, mainTenant);
            if (!createUser.Succeeded)
            {
                Console.WriteLine($"Error creating {mainTenantAdmin}: {string.Join(", ", createUser.Errors.Select(e => e.Description))}");
                return;
            }
        }
        else
        {
            adminUser = existingUser;
        }

        
            
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, mainTenantAdmin, mainTenant.Id))
        {
            await userManager.AddToRoleAsync(adminUser, mainTenantAdmin, mainTenant.Id);
        }

    }
}

