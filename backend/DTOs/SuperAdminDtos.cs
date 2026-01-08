using System.Collections.Generic;

namespace backend.DTOs
{
    public class SuperAdminDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int TotalMerchants { get; set; }
        public int ActiveWashes { get; set; }
        
        // New Statistics
        public int NewMerchantsThisMonth { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public decimal TotalSaaSRevenueAllTime { get; set; }
        public decimal TotalSaaSRevenueThisMonth { get; set; }
        public int ActiveMerchants { get; set; }
        public int InactiveMerchants { get; set; }
        public int BasicPlanCount { get; set; }
        public int ProPlanCount { get; set; }
        public int TotalWashesAllTime { get; set; }
        public int TotalWashesThisMonth { get; set; }
        public int TotalRewardsGiven { get; set; }
        public int TotalRewardsClaimed { get; set; }
        
        // Wash Revenue Statistics (what merchants earn from washes)
        public decimal TotalWashRevenueAllTime { get; set; }
        public decimal TotalWashRevenueThisMonth { get; set; }
        public decimal AvgWashPrice { get; set; }
        
        public SystemStatsDto Stats { get; set; } = new SystemStatsDto();
        public List<AdminActivityDto> RecentActivity { get; set; } = new List<AdminActivityDto>();
    }

    public class SystemStatsDto
    {
        public decimal MonthlyGrowth { get; set; }
        public decimal SystemUptime { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class AdminActivityDto
    {
        public string Icon { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
    }

    public class SuperAdminCustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int TotalWashes { get; set; }
        public string JoinDate { get; set; } = string.Empty;
        public string Status { get; set; } = "active"; // active, blocked
        public bool IsBlocked { get; set; } = false;
    }

    public class SuperAdminMerchantDto
    {
        public string Id { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Plan { get; set; } = "basic"; // basic, pro
        public int Customers { get; set; }
        public int TotalWashes { get; set; }
        public string JoinDate { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending, active, expired, suspended
        public bool IsBlocked { get; set; } = false; // User.IsActive = false means blocked
        public string SubscriptionStartDate { get; set; } = string.Empty;
        public string SubscriptionEndDate { get; set; } = string.Empty;
    }

    public class SystemStatisticsDto
    {
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int InactiveBusinesses { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalWashes { get; set; }
        public int Last30DaysWashes { get; set; }
        public decimal AvgWashesPerDay { get; set; }
        public int TotalRewards { get; set; }
        public int RedeemedRewards { get; set; }
        public int BasicPlanCount { get; set; }
        public int ProPlanCount { get; set; }
    }

    public class PlatformSettingsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public string SupportPhone { get; set; } = string.Empty;
        public decimal BasicPlanPrice { get; set; }
        public decimal ProPlanPrice { get; set; }
        public int TrialPeriod { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool RenewalReminders { get; set; }
        public bool MaintenanceMode { get; set; }
        public string MaintenanceMessage { get; set; } = string.Empty;
    }

    // Request to activate merchant with subscription months
    public class ActivateMerchantRequest
    {
        public int SubscriptionMonths { get; set; } = 1;
    }

    // Request to block/unblock user
    public class BlockUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public bool Block { get; set; } = true;
    }
}
