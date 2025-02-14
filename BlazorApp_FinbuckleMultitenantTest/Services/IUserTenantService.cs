
namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public interface IUserTenantService
    {
        Task<UserTenant> CreateUserTenantAsync(UserTenant userTenant);
        Task<bool> DeleteUserTenantAsync(int id);
        Task<List<UserTenant>> GetAllUserTenantsAsync();
        Task<UserTenant> GetUserTenantAsync(int id);
        Task<UserTenant> UpdateUserTenantAsync(UserTenant userTenant);
    }
}