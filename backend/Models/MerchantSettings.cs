using System;
using System.Collections.Generic;

namespace backend.Models
{
    public class MerchantSettings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MerchantId { get; set; } = string.Empty;
        public Merchant? Merchant { get; set; }
        
        // Loyalty Settings
        public int RewardWashesRequired { get; set; } = 5; // Number of washes to earn reward
        public int RewardTimeLimitDays { get; set; } = 30; // Days to complete required washes
        public string? RewardDescription { get; set; } // What reward the customer gets (set by merchant)
        public string? RewardDescriptionAr { get; set; } // Arabic version of reward description
        public bool AntiFraudSameDay { get; set; } = true; // Prevent multiple washes same day
        public bool EnableCarPhoto { get; set; } = false; // Only for Pro plan
        
        // Loyalty pause feature
        public bool IsLoyaltyPaused { get; set; } = false;
        public DateTime? LoyaltyPausedUntil { get; set; }
        
        // Notification Settings
        public bool NotificationsEnabled { get; set; } = true;
        public string? NotificationTemplateWelcome { get; set; }
        public string? NotificationTemplateRemaining { get; set; }
        public string? NotificationTemplateRewardClose { get; set; }
        
        // Design Settings (for customer-facing pages)
        public string CustomPrimaryColor { get; set; } = "#3B82F6";
        public string CustomSecondaryColor { get; set; } = "#0F172A";
        public string? CustomBusinessTagline { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
