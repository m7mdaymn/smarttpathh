using System;
using System.Collections.Generic;

namespace backend.Models
{
    public class Customer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        
        // Customer QR code - generated on signup for wash tracking
        public string QRCode { get; set; } = string.Empty;
        public DateTime? QRCodeGeneratedAt { get; set; }
        
        // Car details
        public string PlateNumber { get; set; } = string.Empty;
        public string? CarPhoto { get; set; } // Only if merchant has Pro plan with EnableCarPhoto
        
        // Statistics
        public int TotalWashes { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0; // Total amount spent on washes
        public int TotalRewardsEarned { get; set; } = 0;
        public int TotalRewardsClaimed { get; set; } = 0;
        public DateTime? LastWashAt { get; set; }
        
        // First merchant customer registered with
        public string MerchantId { get; set; } = string.Empty;
        public Merchant? Merchant { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        
        // Relations
        public ICollection<LoyaltyCard> LoyaltyCards { get; set; } = new List<LoyaltyCard>();
        public ICollection<WashHistory> WashHistories { get; set; } = new List<WashHistory>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
