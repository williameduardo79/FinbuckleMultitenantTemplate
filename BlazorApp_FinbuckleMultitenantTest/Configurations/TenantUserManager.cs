using BlazorApp_FinbuckleMultitenantTest.Data;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading;
using System.Timers;

namespace BlazorApp_FinbuckleMultitenantTest.Configurations
{
    public class TenantUserManager : UserManager<ApplicationUser>
    {
       
        private readonly IMultiTenantContextAccessor _tenantAccessor;
        private readonly MultiTenantUserStore _store;
       private readonly ILogger<TenantUserManager> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantUserManager(
           MultiTenantUserStore store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger,
            IMultiTenantContextAccessor tenantAccessor,
            IHttpContextAccessor httpContextAccessor
        ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            
            _tenantAccessor = tenantAccessor;
            _store = store;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public async override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Tenant ID is required.");
            //create the user
            var inserted = await base.CreateAsync(user);
            if (!inserted.Succeeded)
            {
                return inserted;
            }

            //var createdUser = await base.FindByEmailAsync(user.Email);
            // Proceed with creating the user for the specific tenant
            var userTenant = new UserTenant
            {
                UserId = user.Id,
                TenantId = tenantId
            };
            CancellationToken cancellationToken = default;
            await _store.CreateUserTenantAsync(userTenant, cancellationToken).ConfigureAwait(false);

            return inserted;

        }
        public async Task<IdentityResult> CreateWithTenantAsync(ApplicationUser user, string password, AppTenantInfo tenant)
        {
            //create the user
          var inserted = await base.CreateAsync(user, password);
            if (!inserted.Succeeded) 
            {
                return inserted;
            }
           
            //var createdUser = await base.FindByEmailAsync(user.Email);
            // Proceed with creating the user for the specific tenant
            var userTenant = new UserTenant
            {
                UserId = user.Id,
                TenantId = tenant.Id
            };
            CancellationToken cancellationToken = default;
            await _store.CreateUserTenantAsync(userTenant, cancellationToken).ConfigureAwait(false);

            return inserted;
        }
        public async Task<IdentityResult> CreateWithTenantAsync(ApplicationUser user, AppTenantInfo tenant)
        {
            //create the user
            var inserted = await base.CreateAsync(user);
            if (!inserted.Succeeded)
            {
                return inserted;
            }

            //var createdUser = await base.FindByEmailAsync(user.Email);
            // Proceed with creating the user for the specific tenant
            var userTenant = new UserTenant
            {
                UserId = user.Id,
                TenantId = tenant.Id
            };
            CancellationToken cancellationToken = default;
            await _store.CreateUserTenantAsync(userTenant, cancellationToken).ConfigureAwait(false);

            return inserted;
        }

       

        public override async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            return await IsInRoleAsync(user,roleName,tenantId);
        }
        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, string tenantId)
        {
           
            if (tenantId == null) throw new InvalidOperationException("Tenant ID is required.");

            return await _store.IsInRoleAsync(user.Id, tenantId, roleName);
        }
        public override async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
          
            return await AddToRoleAsync(user,roleName,tenantId);
        }
        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, string tenantId)
        {
            try
            {
                await _store.AddToRoleAsync(user, roleName, tenantId );
                return IdentityResult.Success;
            }
            catch (Exception ex) 
            {
                return IdentityResult.Failed(new IdentityError { Code="roleExc", Description=ex.Message });
            }

          
        }
        public async Task<IList<UserTenantRole>> GetAllUserRoles()
        {
            return await _store.GetAllUserRoles();
        }
        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, string tenantId)
        {
            
            return await _store.GetRolesAsync(user, tenantId);
        }
        public override async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            
            return await GetRolesAsync(user, tenantId);
        }
        public override async Task<IdentityResult> AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims)
        {
            try
            {
                var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
                await _store.AddClaimsAsync(user, claims, tenantId);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "claimExc", Description = ex.Message });
            }
           
        }
        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            return await _store.GetClaimsAsync(user, tenantId);
        }
    }

}
