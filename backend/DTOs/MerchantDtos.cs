using System;
using System.Collections.Generic;

namespace backend.DTOs
{
    public class MerchantProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Plan { get; set; } = "Basic";
        public DateTime PlanExpiryDate { get; set; }
        public string SubscriptionStatus { get; set; } = "pending";
        public int TotalCustomers { get; set; }
        public int TotalWashes { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? RegistrationCode { get; set; }
    }

    public class MerchantDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersToday { get; set; }
        public int WashesToday { get; set; }
        public string LastWashTime { get; set; } = "لا يوجد";
        public int RewardsGiven { get; set; }
        public int PendingRewards { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalWashesAllTime { get; set; }
        
        // Weekly stats
        public decimal WeeklyRevenue { get; set; }
        public int WashesThisWeek { get; set; }
        public int NewCustomersThisWeek { get; set; }
        
        // Monthly stats
        public decimal MonthlyRevenue { get; set; }
        public int WashesThisMonth { get; set; }
        public int NewCustomersThisMonth { get; set; }
        
        public string Plan { get; set; } = "Basic";
        public string SubscriptionStatus { get; set; } = "pending";
        public DateTime? PlanExpiryDate { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? RegistrationCode { get; set; }
        public List<ActivityDto> RecentActivity { get; set; } = new List<ActivityDto>();
    }

    public class ActivityDto
    {
        public string Type { get; set; } = string.Empty; // wash, customer, reward
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class MerchantCustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CarPhoto { get; set; }
        public string? PlateNumber { get; set; }
        public DateTime JoinDate { get; set; }
        public int CurrentWashes { get; set; }  // Current progress toward reward
        public int TotalWashesRequired { get; set; }  // Required for reward
        public int TotalWashes { get; set; }  // All-time washes with this merchant
        public int DaysLeft { get; set; }
        public DateTime? LastWash { get; set; }  // Last wash date
        public string Status { get; set; } = "active"; // active, inactive
    }

    public class MerchantSettingsDto
    {
        public string Id { get; set; } = string.Empty;
        public int RewardWashesRequired { get; set; } = 5;
        public int RewardTimeLimitDays { get; set; } = 30;
        public string? RewardDescription { get; set; }
        public string? RewardDescriptionAr { get; set; }
        public bool AntiFraudSameDay { get; set; } = true;
        public bool EnableCarPhoto { get; set; } = false;
        public bool IsLoyaltyPaused { get; set; } = false;
        public DateTime? LoyaltyPausedUntil { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public string? NotificationTemplateWelcome { get; set; }
        public string? NotificationTemplateRemaining { get; set; }
        public string? NotificationTemplateRewardClose { get; set; }
        public string CustomPrimaryColor { get; set; } = "#3B82F6";
        public string CustomSecondaryColor { get; set; } = "#0F172A";
        public string? CustomBusinessTagline { get; set; }
    }

    public class QRScanResultDto
    {
        public bool Success { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerPhoto { get; set; }
        public string? PlateNumber { get; set; }
        public int CurrentWashes { get; set; }
        public int TotalWashesRequired { get; set; }
        public decimal Progress { get; set; }
        public int DaysLeft { get; set; }
        public bool RewardEarned { get; set; }
        public string? RewardDescription { get; set; }
    }

    public class ProcessWashRequest
    {
        public string CustomerQRCode { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public decimal Price { get; set; }
    }

    public class RewardValidationDto
    {
        public bool Success { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string RewardTitle { get; set; } = string.Empty;
        public string RewardType { get; set; } = "free_wash";
        public decimal RewardValue { get; set; }
        public DateTime? RewardExpiresAt { get; set; }
        public bool IsAlreadyClaimed { get; set; }
        public bool IsExpired { get; set; }
    }

    public class MerchantCustomerUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class ValidateCustomerQRRequest
    {
        public string CustomerQRCode { get; set; } = string.Empty;
    }
}
