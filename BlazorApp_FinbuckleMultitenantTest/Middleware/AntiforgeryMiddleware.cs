using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorApp_FinbuckleMultitenantTest.Middleware
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;
        IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;
        public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery, IMultiTenantContextAccessor<AppTenantInfo> multiTenantContext)
        {
            _next = next;
            _antiforgery = antiforgery;
            _tenantAccessor = multiTenantContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Detect if tenant/user has changed
            var tenantChanged = context.Request.Cookies["TenantId"] != context.Session.GetString("TenantId");

            if (tenantChanged)
            {
                // Remove old token cookies
                var antiforgeryCookieName = ".AspNetCore.Antiforgery"; // Default name, may vary per app config
                context.Response.Cookies.Delete(antiforgeryCookieName);
                context.Response.Cookies.Delete(".AspNetCore.Identity.Application");

                // Regenerate new token
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append(antiforgeryCookieName, tokens.RequestToken,
                    new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });

                // Update session to track tenant change
                var tenantInfo = _tenantAccessor.MultiTenantContext?.TenantInfo;
                if (tenantInfo != null)
                {
                    context.Session.SetString("TenantId", tenantInfo.Identifier);
                }

                  
            }

            await _next(context);
        }
    }
}
