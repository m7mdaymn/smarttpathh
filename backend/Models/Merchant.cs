using System;
using System.Collections.Generic;

namespace backend.Models
{
    public class Merchant
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = "car_wash";
        public string City { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        
        // Subscription
        public string Plan { get; set; } = "Basic"; // Basic, Pro
        public DateTime PlanExpiryDate { get; set; }
        public string SubscriptionStatus { get; set; } = "pending"; // pending, active, expired, suspended
        
        // Statistics
        public int TotalCustomers { get; set; } = 0;
        public int TotalWashes { get; set; } = 0;
        public int TotalRevenue { get; set; } = 0; // In cents calculated from each wash price
        public int TotalRewardsGiven { get; set; } = 0;
        
        // Features based on plan (Pro = unlimited, Basic = 50 users max)
        public bool HasNotificationsEnabled { get; set; } = false;
        
        // QR Code for customer registration - generated on activation
        public string? RegistrationCode { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public DateTime? QRCodeGeneratedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Relations
        public ICollection<LoyaltyCard> LoyaltyCards { get; set; } = new List<LoyaltyCard>();
        public ICollection<MerchantSettings> Settings { get; set; } = new List<MerchantSettings>();
        public ICollection<WashHistory> WashHistories { get; set; } = new List<WashHistory>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        
        public static string GenerateRegistrationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        
        // Helper: Check if merchant can add more customers based on plan
        public bool CanAddMoreCustomers()
        {
            if (Plan.ToLower() == "pro") return true;
            return TotalCustomers < 50; // Basic plan limit
        }
    }
}
