using System;

namespace backend.Models
{
    public class LoyaltyCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
        
        public string MerchantId { get; set; } = string.Empty;
        public Merchant? Merchant { get; set; }
        
        // Progress tracking
        public int WashesCompleted { get; set; } = 0;
        public int WashesRequired { get; set; } = 4; // Copied from merchant settings at creation
        public DateTime ExpiresAt { get; set; } // Based on merchant's RewardTimeLimitDays
        
        // Status
        public bool IsActive { get; set; } = true;
        public bool IsPaused { get; set; } = false; // Follows merchant's loyalty pause setting
        
        // Reward status - when WashesCompleted >= WashesRequired
        public bool IsRewardEarned { get; set; } = false;
        public DateTime? RewardEarnedAt { get; set; }
        public string? RewardQRCode { get; set; } // Generated when reward is earned
        public bool IsRewardClaimed { get; set; } = false;
        public DateTime? RewardClaimedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
