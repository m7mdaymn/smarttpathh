using System;

namespace backend.Models
{
    // Rewards are earned when a customer completes a loyalty card
    // The reward description/terms are set in MerchantSettings, not per-reward
    public class Reward
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
        
        public string MerchantId { get; set; } = string.Empty;
        public Merchant? Merchant { get; set; }
        
        public string LoyaltyCardId { get; set; } = string.Empty; // Link to the loyalty card that earned this reward
        
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "available"; // available, claimed, expired
        
        public string? RewardQRCode { get; set; } // QR code for redemption
        public DateTime? ClaimedAt { get; set; }
        public string? ClaimedByMerchantId { get; set; } // Which merchant branch claimed it
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
