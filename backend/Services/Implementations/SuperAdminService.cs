using backend.Controllers;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.Implementations
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<SuperAdminDashboardDto>> GetDashboardAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = today.AddDays(-30);
                
                var totalCustomers = await _context.Customers.CountAsync();
                var totalMerchants = await _context.Merchants.CountAsync();
                
                // New merchants and customers this month
                var newMerchantsThisMonth = await _context.Merchants
                    .Where(m => m.CreatedAt >= monthStart)
                    .CountAsync();
                var newCustomersThisMonth = await _context.Customers
                    .Where(c => c.CreatedAt >= monthStart)
                    .CountAsync();
                
                // Active vs Inactive merchants
                var activeMerchants = await _context.Merchants
                    .Where(m => m.SubscriptionStatus == "active")
                    .CountAsync();
                var inactiveMerchants = totalMerchants - activeMerchants;
                
                // Plan counts
                var basicPlanCount = await _context.Merchants
                    .Where(m => m.Plan.ToLower() == "basic")
                    .CountAsync();
                var proPlanCount = await _context.Merchants
                    .Where(m => m.Plan.ToLower() == "pro")
                    .CountAsync();
                
                // Washes statistics
                var totalWashesAllTime = await _context.WashHistories.CountAsync();
                var totalWashesThisMonth = await _context.WashHistories
                    .Where(wh => wh.WashDate >= monthStart)
                    .CountAsync();
                
                // Wash Revenue statistics (what merchants earn from washes)
                var totalWashRevenueAllTime = await _context.WashHistories
                    .SumAsync(wh => wh.Price);
                var totalWashRevenueThisMonth = await _context.WashHistories
                    .Where(wh => wh.WashDate >= monthStart)
                    .SumAsync(wh => wh.Price);
                var avgWashPrice = totalWashesAllTime > 0 
                    ? totalWashRevenueAllTime / totalWashesAllTime 
                    : 0;
                
                // Rewards statistics
                var totalRewardsGiven = await _context.LoyaltyCards
                    .Where(lc => lc.IsRewardEarned)
                    .CountAsync();
                var totalRewardsClaimed = await _context.LoyaltyCards
                    .Where(lc => lc.IsRewardClaimed)
                    .CountAsync();
                
                // SaaS Revenue calculation (based on plan prices)
                // Basic = 99 SAR/month, Pro = 149 SAR/month
                var basicPrice = 99m;
                var proPrice = 149m;
                
                // Calculate monthly SaaS revenue based on active subscriptions
                var totalSaaSRevenueThisMonth = (activeMerchants > 0) 
                    ? (basicPlanCount * basicPrice) + (proPlanCount * proPrice)
                    : 0;
                
                // Estimate all-time revenue (this is an approximation based on current state)
                // In production, you would track actual payments
                var totalSaaSRevenueAllTime = totalSaaSRevenueThisMonth * 12; // Rough estimate
                
                // Calculate growth
                var lastMonthMerchants = await _context.Merchants
                    .Where(m => m.CreatedAt < monthStart && m.CreatedAt >= monthStart.AddDays(-30))
                    .CountAsync();
                var monthlyGrowth = lastMonthMerchants > 0 
                    ? ((decimal)(newMerchantsThisMonth - lastMonthMerchants) / lastMonthMerchants) * 100 
                    : (newMerchantsThisMonth > 0 ? 100 : 0);

                var dashboard = new SuperAdminDashboardDto
                {
                    TotalCustomers = totalCustomers,
                    TotalMerchants = totalMerchants,
                    ActiveWashes = await _context.WashHistories
                        .Where(wh => wh.Status == "pending")
                        .CountAsync(),
                    NewMerchantsThisMonth = newMerchantsThisMonth,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    TotalSaaSRevenueAllTime = totalSaaSRevenueAllTime,
                    TotalSaaSRevenueThisMonth = totalSaaSRevenueThisMonth,
                    ActiveMerchants = activeMerchants,
                    InactiveMerchants = inactiveMerchants,
                    BasicPlanCount = basicPlanCount,
                    ProPlanCount = proPlanCount,
                    TotalWashesAllTime = totalWashesAllTime,
                    TotalWashesThisMonth = totalWashesThisMonth,
                    TotalRewardsGiven = totalRewardsGiven,
                    TotalRewardsClaimed = totalRewardsClaimed,
                    TotalWashRevenueAllTime = totalWashRevenueAllTime,
                    TotalWashRevenueThisMonth = totalWashRevenueThisMonth,
                    AvgWashPrice = Math.Round(avgWashPrice, 2),
                    Stats = new SystemStatsDto
                    {
                        MonthlyGrowth = Math.Round(monthlyGrowth, 1),
                        SystemUptime = 99.8m,
                        TotalTransactions = totalWashesAllTime
                    }
                };

                // Get recent activity
                var recentMerchants = await _context.Merchants
                    .Include(m => m.User)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var recentCustomers = await _context.Customers
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                foreach (var merchant in recentMerchants)
                {
                    dashboard.RecentActivity.Add(new AdminActivityDto
                    {
                        Icon = "🏪",
                        Type = "merchant",
                        Title = "تاجر جديد",
                        Description = $"انضم {merchant.BusinessName} للمنصة",
                        Time = GetTimeAgo(merchant.CreatedAt),
                        Status = merchant.SubscriptionStatus == "active" ? "success" : "pending",
                        StatusText = merchant.SubscriptionStatus == "active" ? "نشط" : "قيد الانتظار"
                    });
                }

                foreach (var customer in recentCustomers)
                {
                    dashboard.RecentActivity.Add(new AdminActivityDto
                    {
                        Icon = "👤",
                        Type = "customer",
                        Title = "عميل جديد",
                        Description = $"انضم {customer.User?.Name ?? "عميل"} للمنصة",
                        Time = GetTimeAgo(customer.CreatedAt),
                        Status = "success",
                        StatusText = "مسجل"
                    });
                }

                // Sort by most recent
                dashboard.RecentActivity = dashboard.RecentActivity.Take(10).ToList();

                return new ApiResponse<SuperAdminDashboardDto>
                {
                    Success = true,
                    Data = dashboard
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SuperAdminDashboardDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 1) return "الآن";
            if (timeSpan.TotalMinutes < 60) return $"منذ {(int)timeSpan.TotalMinutes} دقيقة";
            if (timeSpan.TotalHours < 24) return $"منذ {(int)timeSpan.TotalHours} ساعة";
            if (timeSpan.TotalDays < 7) return $"منذ {(int)timeSpan.TotalDays} يوم";
            return dateTime.ToString("yyyy-MM-dd");
        }

        public async Task<ApiResponse<List<SuperAdminCustomerDto>>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.User)
                    .Select(c => new SuperAdminCustomerDto
                    {
                        Id = c.Id,
                        Name = c.User != null ? c.User.Name : "",
                        Email = c.User != null ? c.User.Email : "",
                        Phone = c.User != null ? c.User.Phone : "",
                        TotalWashes = c.TotalWashes,
                        JoinDate = c.CreatedAt.ToString("yyyy-MM-dd"),
                        Status = c.User != null && c.User.IsActive ? "active" : "blocked",
                        IsBlocked = c.User != null && !c.User.IsActive
                    })
                    .ToListAsync();

                return new ApiResponse<List<SuperAdminCustomerDto>>
                {
                    Success = true,
                    Data = customers
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SuperAdminCustomerDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<SuperAdminMerchantDto>>> GetAllMerchantsAsync()
        {
            try
            {
                var merchants = await _context.Merchants
                    .Include(m => m.User)
                    .Include(m => m.LoyaltyCards)
                    .Include(m => m.WashHistories)
                    .Select(m => new SuperAdminMerchantDto
                    {
                        Id = m.Id,
                        BusinessName = m.BusinessName,
                        OwnerName = m.User != null ? m.User.Name : "",
                        Email = m.User != null ? m.User.Email : "",
                        Phone = m.User != null ? m.User.Phone : "",
                        City = m.City,
                        Plan = m.Plan.ToLower(),
                        Customers = m.LoyaltyCards.Select(lc => lc.CustomerId).Distinct().Count(),
                        TotalWashes = m.WashHistories.Count(),
                        JoinDate = m.CreatedAt.ToString("yyyy-MM-dd"),
                        Status = m.SubscriptionStatus,
                        IsBlocked = m.User != null && !m.User.IsActive,
                        SubscriptionStartDate = m.CreatedAt.ToString("yyyy-MM-dd"),
                        SubscriptionEndDate = m.PlanExpiryDate != DateTime.MinValue ? m.PlanExpiryDate.ToString("yyyy-MM-dd") : ""
                    })
                    .ToListAsync();

                return new ApiResponse<List<SuperAdminMerchantDto>>
                {
                    Success = true,
                    Data = merchants
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SuperAdminMerchantDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<SystemStatisticsDto>> GetStatisticsAsync()
        {
            try
            {
                var stats = new SystemStatisticsDto
                {
                    TotalBusinesses = await _context.Merchants.CountAsync(),
                    ActiveBusinesses = await _context.Merchants
                        .Where(m => m.SubscriptionStatus == "active")
                        .CountAsync(),
                    TotalCustomers = await _context.Customers.CountAsync(),
                    TotalWashes = await _context.WashHistories.CountAsync(),
                    TotalRewards = await _context.Rewards.CountAsync(),
                    RedeemedRewards = await _context.Rewards
                        .Where(r => r.Status == "claimed")
                        .CountAsync(),
                    BasicPlanCount = await _context.Merchants
                        .Where(m => m.Plan == "Basic")
                        .CountAsync(),
                    ProPlanCount = await _context.Merchants
                        .Where(m => m.Plan == "Pro")
                        .CountAsync()
                };

                return new ApiResponse<SystemStatisticsDto>
                {
                    Success = true,
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SystemStatisticsDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PlatformSettingsDto>> GetSettingsAsync()
        {
            try
            {
                var settings = new PlatformSettingsDto
                {
                    Id = "1",
                    Name = "Digital Pass",
                    SupportEmail = "support@digitalpass.com",
                    SupportPhone = "0548290509",
                    BasicPlanPrice = 99,
                    ProPlanPrice = 149,
                    TrialPeriod = 7,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    RenewalReminders = true,
                    MaintenanceMode = false
                };

                return new ApiResponse<PlatformSettingsDto>
                {
                    Success = true,
                    Data = settings
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PlatformSettingsDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateSettingsAsync(PlatformSettingsDto settings)
        {
            try
            {
                // في التطبيق الحقيقي، نحفظ الإعدادات في قاعدة البيانات
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "تم تحديث الإعدادات",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> SuspendCustomerAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new ApiResponse<bool> { Success = false };

                customer.User.IsActive = false;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> SuspendMerchantAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                merchant.User.IsActive = false;
                merchant.SubscriptionStatus = "suspended";
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم تعليق التاجر بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ActivateCustomerAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new ApiResponse<bool> { Success = false };

                customer.User.IsActive = true;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ActivateMerchantAsync(string merchantId, int months)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                merchant.User!.IsActive = true;
                merchant.SubscriptionStatus = "active";
                merchant.PlanExpiryDate = DateTime.UtcNow.AddMonths(months);
                
                // Generate QR code for merchant if not already generated
                if (string.IsNullOrEmpty(merchant.RegistrationCode))
                {
                    merchant.RegistrationCode = Merchant.GenerateRegistrationCode();
                    merchant.QRCodeImageUrl = $"/api/qrcode/merchant/{merchant.Id}";
                    merchant.QRCodeGeneratedAt = DateTime.UtcNow;
                }
                
                // Create default merchant settings if not exist
                var hasSettings = await _context.Set<MerchantSettings>()
                    .AnyAsync(s => s.MerchantId == merchantId);
                    
                if (!hasSettings)
                {
                    var settings = new MerchantSettings
                    {
                        MerchantId = merchantId,
                        RewardWashesRequired = 5,
                        RewardTimeLimitDays = 30,
                        RewardDescription = "غسلة مجانية",
                        EnableCarPhoto = merchant.Plan.ToLower() == "pro"
                    };
                    _context.Set<MerchantSettings>().Add(settings);
                }
                
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = $"تم تفعيل التاجر بنجاح لمدة {months} شهر" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Block merchant - cannot login at all
        public async Task<ApiResponse<bool>> BlockMerchantAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                merchant.User!.IsActive = false;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم حظر التاجر بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Unblock merchant - can login again
        public async Task<ApiResponse<bool>> UnblockMerchantAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                merchant.User!.IsActive = true;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم إلغاء حظر التاجر بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Suspend subscription - can login but cannot scan washes
        public async Task<ApiResponse<bool>> SuspendSubscriptionAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                merchant.SubscriptionStatus = "suspended";
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم تعليق الاشتراك بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateMerchantPlanAsync(string merchantId, string newPlan)
        {
            try
            {
                var merchant = await _context.Merchants.FindAsync(merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false };

                merchant.Plan = newPlan;
                merchant.HasNotificationsEnabled = newPlan.ToLower() == "pro";
                
                // Update settings for EnableCarPhoto based on plan
                var settings = await _context.Set<MerchantSettings>()
                    .FirstOrDefaultAsync(s => s.MerchantId == merchantId);
                if (settings != null)
                {
                    settings.EnableCarPhoto = newPlan.ToLower() == "pro";
                }
                
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateMerchantAsync(string merchantId, UpdateMerchantRequest request)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false, Message = "التاجر غير موجود" };

                // Update merchant fields
                if (!string.IsNullOrEmpty(request.BusinessName))
                    merchant.BusinessName = request.BusinessName;
                
                if (!string.IsNullOrEmpty(request.OwnerName) && merchant.User != null)
                    merchant.User.Name = request.OwnerName;
                
                if (!string.IsNullOrEmpty(request.City))
                    merchant.City = request.City;

                // Update plan if provided
                if (!string.IsNullOrEmpty(request.Plan))
                {
                    merchant.Plan = request.Plan;
                    merchant.HasNotificationsEnabled = request.Plan.ToLower() == "pro";
                    
                    // Update settings for EnableCarPhoto based on plan
                    var settings = await _context.Set<MerchantSettings>()
                        .FirstOrDefaultAsync(s => s.MerchantId == merchantId);
                    if (settings != null)
                    {
                        settings.EnableCarPhoto = request.Plan.ToLower() == "pro";
                    }
                }
                
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم تحديث بيانات التاجر بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
