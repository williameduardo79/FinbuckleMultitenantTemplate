using BlazorApp_FinbuckleMultitenantTest.Data;

namespace BlazorApp_FinbuckleMultitenantTest.Services
{
    public interface IAppTenantInfoService
    {
        Task<AppTenantInfo> CreateAsync(AppTenantInfo tenant);
        Task<bool> DeleteAsync(string id);
        Task<List<AppTenantInfo>> GetAllAsync();
        Task<AppTenantInfo?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(AppTenantInfo tenant);
    }
}