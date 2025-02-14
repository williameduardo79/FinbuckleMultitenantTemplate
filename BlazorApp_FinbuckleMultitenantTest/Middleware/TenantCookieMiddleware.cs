using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Middleware
{
    public class TenantCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IMultiTenantContextAccessor multiTenantContext)
        {
            var tenantInfo = multiTenantContext.MultiTenantContext?.TenantInfo;

            if (tenantInfo != null)
            {
                var cookieName = $".AspNetCore.Identity.{tenantInfo.Identifier}";
                context.Response.Cookies.Append(cookieName, "true", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                var tenantCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                context.Response.Cookies.Append("TenantId", tenantInfo.Identifier, tenantCookieOptions);
            }

            await _next(context);
        }
    }

}
