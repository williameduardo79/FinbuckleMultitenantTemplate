﻿using BlazorApp_FinbuckleMultitenantTest.Components;
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
})
    .AddEntityFrameworkStores<ApplicationDbContext>() //Ensures EF Core stores roles
    .AddSignInManager<SignInManager<ApplicationUser>>() //Adds SignInManager
     .AddUserManager<UserManager<ApplicationUser>>()
    .AddDefaultTokenProviders()
    .AddUserStore<MultiTenantUserStore>(); //Ensures token support (e.g., for password reset)


builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();
builder.Services.AddScoped<IUserStore<ApplicationUser>, MultiTenantUserStore>();



builder.Services.AddHttpContextAccessor();

// Add the TenantDbContext for tenant-specific data access
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TenantConnection")));

//Finbuckle Multitenant
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithStore<CustomTenantStore>(ServiceLifetime.Scoped)
    .WithHostStrategy();


builder.Services.AddScoped<IUserTenantService, UserTenantService>();
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
app.UseSession(); // This should come before UseRouting or other middleware
app.UseStaticFiles();
app.UseAntiforgery();
app.UseMiddleware<TenantCookieMiddleware>();
app.UseMiddleware<AntiforgeryMiddleware>();
app.UseAuthentication(); // Authentication must come before Authorization
app.UseAuthorization();  // Authorization must follow authentication

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
