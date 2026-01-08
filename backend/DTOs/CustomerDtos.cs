using System;

namespace backend.DTOs
{
    public class CustomerProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string? PlateNumber { get; set; }
        public string? CarPhoto { get; set; }
        public int TotalWashes { get; set; }
        public List<LoyaltyCardDto> LoyaltyCards { get; set; } = new List<LoyaltyCardDto>();
        public List<WashHistoryDto> Washes { get; set; } = new List<WashHistoryDto>();
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
    }

    public class LoyaltyCardDto
    {
        public string Id { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public int WashesCompleted { get; set; }
        public int WashesRequired { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }
        public decimal Progress { get; set; }
        
        // Merchant Settings
        public string? RewardDescription { get; set; }
        public string? MerchantCity { get; set; }
        public string? MerchantPhone { get; set; }
        public string? CustomPrimaryColor { get; set; }
        public int? RewardTimeLimitDays { get; set; }
        public bool AllowCarPhotoUpload { get; set; }
        
        // Reward Status
        public bool IsRewardEarned { get; set; }
        public string? RewardQRCode { get; set; }
        public DateTime? RewardEarnedAt { get; set; }
        public bool IsRewardClaimed { get; set; }
        public DateTime? RewardClaimedAt { get; set; }
        
        // Statistics
        public int DaysRemaining { get; set; }
        public int TotalWashesWithMerchant { get; set; }
        public DateTime? LastWashDate { get; set; }
    }

    public class WashHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public DateTime WashDate { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "completed";
        public int? Rating { get; set; }
        public string? CustomerComment { get; set; }
    }

    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RewardDto
    {
        public string Id { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string? RewardDescription { get; set; } // From merchant settings
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "available";
        public string? RewardQRCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class RateWashRequest
    {
        public string WashId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }

    public class UpdateCarPhotoRequest
    {
        public string? CarPhoto { get; set; }
    }
}
