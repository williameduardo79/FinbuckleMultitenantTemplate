using Finbuckle.MultiTenant.Abstractions;

namespace BlazorApp_FinbuckleMultitenantTest.Middleware
{
    public class TenantCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _errorPageUrl = "/Error"; // Redirect URL

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
            //Send to error for non existant tenants
            if (tenantInfo == null && !context.Request.Path.StartsWithSegments(_errorPageUrl, StringComparison.OrdinalIgnoreCase))
            {
                // 🔹 Redirect to the error page with a query parameter
                context.Response.Redirect($"{_errorPageUrl}?error=NoTenantFound");
                //context.Request
                return;
            }
           
           
            

            await _next(context);
        }
    }

}
