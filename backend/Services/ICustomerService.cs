using backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ICustomerService
    {
        Task<ApiResponse<CustomerProfileDto>> GetCustomerProfileAsync(string customerId);
        Task<ApiResponse<List<LoyaltyCardDto>>> GetLoyaltyCardsAsync(string customerId);
        Task<ApiResponse<List<WashHistoryDto>>> GetWashHistoryAsync(string customerId);
        Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(string customerId);
        Task<ApiResponse<List<RewardDto>>> GetRewardsAsync(string customerId);
        Task<ApiResponse<bool>> MarkNotificationAsReadAsync(string notificationId);
        Task<ApiResponse<bool>> ClaimRewardAsync(string customerId, string rewardId);
        Task<ApiResponse<bool>> RateWashAsync(string washId, int rating, string comments);
        Task<string> GetCustomerIdByUserIdAsync(string userId);
        Task<ApiResponse<bool>> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequest request);
    }

    public class UpdateCustomerProfileRequest
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? PlateNumber { get; set; }
    }
}
