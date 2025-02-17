using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorApp_FinbuckleMultitenantTest.Middleware
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;
        private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;

        public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery, IMultiTenantContextAccessor<AppTenantInfo> multiTenantContext)
        {
            _next = next;
            _antiforgery = antiforgery;
            _tenantAccessor = multiTenantContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantInfo = _tenantAccessor.MultiTenantContext?.TenantInfo;
            var tenantId = tenantInfo?.Identifier ?? "Default";
            var antiforgeryCookieName = $".AspNetCore.Antiforgery.{tenantId}";

            var currentSessionTenantId = context.Session.GetString("TenantId");
            var currentSessionUser = context.Session.GetString("UserName");
            var loggedInUser = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : null;

            bool tenantChanged = currentSessionTenantId != tenantId;
            bool userChanged = loggedInUser != currentSessionUser;

            if (tenantChanged || userChanged)
            {
                // Only clear session if a logged-in user actually changed (avoid clearing on null user)
                if (userChanged && loggedInUser != null)
                {
                    context.Session.Clear();
                    context.Session.SetString("UserName", loggedInUser);
                }

                // Remove only the tenant-specific antiforgery cookie
                context.Response.Cookies.Delete(antiforgeryCookieName);

                // Generate a new antiforgery token
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append(antiforgeryCookieName, tokens.RequestToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                // Store the new tenant ID
                context.Session.SetString("TenantId", tenantId);
            }

            await _next(context);
        }
    }


}
