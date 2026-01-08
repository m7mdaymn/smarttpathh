using backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IMerchantService
    {
        Task<ApiResponse<string>> GetMerchantIdByUserIdAsync(string userId);
        Task<ApiResponse<MerchantProfileDto>> GetMerchantProfileAsync(string merchantId);
        Task<ApiResponse<MerchantDashboardDto>> GetDashboardAsync(string merchantId);
        Task<ApiResponse<List<MerchantCustomerDto>>> GetCustomersAsync(string merchantId);
        Task<ApiResponse<MerchantSettingsDto>> GetSettingsAsync(string merchantId);
        Task<ApiResponse<bool>> UpdateSettingsAsync(string merchantId, MerchantSettingsDto settings);
        Task<ApiResponse<QRScanResultDto>> ProcessQRScanAsync(ProcessWashRequest request);
        Task<ApiResponse<QRScanResultDto>> GetCustomerByQRCodeAsync(string merchantId, string customerQRCode); // Validate only, no wash record
        Task<ApiResponse<QRScanResultDto>> RecordWashAsync(ProcessWashRequest request); // Record wash only after validation
        Task<ApiResponse<bool>> CreateLoyaltyCardAsync(string merchantId, string customerId);
        Task<ApiResponse<bool>> UpdatePasswordAsync(string merchantId, string currentPassword, string newPassword);
        Task<ApiResponse<bool>> UpdateProfileAsync(string merchantId, MerchantProfileDto profile);
        Task<ApiResponse<string>> UploadLogoAsync(string merchantId, string logoUrl);
        Task<ApiResponse<RewardValidationDto>> ValidateRewardQRCodeAsync(string merchantId, string rewardQRCode);
        Task<ApiResponse<bool>> RedeemRewardAsync(string merchantId, string rewardQRCode);
        // Customer CRUD operations
        Task<ApiResponse<bool>> UpdateCustomerAsync(string merchantId, string customerId, MerchantCustomerUpdateDto customerData);
        Task<ApiResponse<bool>> DeleteCustomerAsync(string merchantId, string customerId);
        Task<ApiResponse<bool>> ToggleCustomerStatusAsync(string merchantId, string customerId, bool activate);
    }
}
