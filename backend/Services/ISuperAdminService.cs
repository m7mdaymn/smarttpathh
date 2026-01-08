using backend.Controllers;
using backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ISuperAdminService
    {
        Task<ApiResponse<SuperAdminDashboardDto>> GetDashboardAsync();
        Task<ApiResponse<List<SuperAdminCustomerDto>>> GetAllCustomersAsync();
        Task<ApiResponse<List<SuperAdminMerchantDto>>> GetAllMerchantsAsync();
        Task<ApiResponse<SystemStatisticsDto>> GetStatisticsAsync();
        Task<ApiResponse<PlatformSettingsDto>> GetSettingsAsync();
        Task<ApiResponse<bool>> UpdateSettingsAsync(PlatformSettingsDto settings);
        Task<ApiResponse<bool>> SuspendCustomerAsync(string customerId);
        Task<ApiResponse<bool>> SuspendMerchantAsync(string merchantId);
        Task<ApiResponse<bool>> ActivateCustomerAsync(string customerId);
        Task<ApiResponse<bool>> ActivateMerchantAsync(string merchantId, int months);
        Task<ApiResponse<bool>> UpdateMerchantPlanAsync(string merchantId, string newPlan);
        Task<ApiResponse<bool>> UpdateMerchantAsync(string merchantId, UpdateMerchantRequest request);
        
        // Block/Unblock - prevents login completely
        Task<ApiResponse<bool>> BlockMerchantAsync(string merchantId);
        Task<ApiResponse<bool>> UnblockMerchantAsync(string merchantId);
        
        // Suspend subscription - can login but cannot scan washes
        Task<ApiResponse<bool>> SuspendSubscriptionAsync(string merchantId);
    }
}
