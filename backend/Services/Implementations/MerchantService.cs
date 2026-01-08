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
    public class MerchantService : IMerchantService
    {
        private readonly ApplicationDbContext _context;

        public MerchantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<string>> GetMerchantIdByUserIdAsync(string userId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.UserId == userId);

                if (merchant == null)
                    return new ApiResponse<string> { Success = false, Message = "لم يتم العثور على مغسلة مرتبطة بهذا المستخدم" };

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = merchant.Id
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<MerchantProfileDto>> GetMerchantProfileAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<MerchantProfileDto> { Success = false, Message = "التاجر غير موجود" };

                var profile = new MerchantProfileDto
                {
                    Id = merchant.Id,
                    BusinessName = merchant.BusinessName,
                    City = merchant.City,
                    Phone = merchant.User?.Phone ?? "",
                    Email = merchant.User?.Email ?? "",
                    Plan = merchant.Plan,
                    PlanExpiryDate = merchant.PlanExpiryDate,
                    SubscriptionStatus = merchant.SubscriptionStatus,
                    TotalCustomers = merchant.TotalCustomers,
                    TotalWashes = merchant.TotalWashes,
                    QRCodeImageUrl = merchant.QRCodeImageUrl,
                    RegistrationCode = merchant.RegistrationCode
                };

                return new ApiResponse<MerchantProfileDto>
                {
                    Success = true,
                    Data = profile
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantProfileDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<MerchantDashboardDto>> GetDashboardAsync(string merchantId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                
                var merchant = await _context.Merchants.FindAsync(merchantId);
                if (merchant == null)
                    return new ApiResponse<MerchantDashboardDto> { Success = false, Message = "التاجر غير موجود" };

                var totalCustomers = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.IsActive)
                    .Select(lc => lc.CustomerId)
                    .Distinct()
                    .CountAsync();

                var newCustomersToday = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.CreatedAt.Date == today)
                    .CountAsync();

                var washesToday = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date == today)
                    .CountAsync();

                var rewardsGiven = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.IsRewardEarned)
                    .CountAsync();

                var pendingRewards = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.IsRewardEarned && !lc.IsRewardClaimed)
                    .CountAsync();

                var lastWash = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId)
                    .OrderByDescending(wh => wh.WashDate)
                    .FirstOrDefaultAsync();

                // Calculate revenue from washes
                var todayRevenue = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date == today)
                    .SumAsync(wh => wh.Price);

                var totalRevenue = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId)
                    .SumAsync(wh => wh.Price);

                var totalWashesAllTime = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId)
                    .CountAsync();

                // Calculate weekly stats (last 7 days)
                var weekStart = today.AddDays(-7);
                var weeklyRevenue = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date >= weekStart)
                    .SumAsync(wh => wh.Price);
                
                var washesThisWeek = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date >= weekStart)
                    .CountAsync();
                
                var newCustomersThisWeek = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.CreatedAt.Date >= weekStart)
                    .CountAsync();

                // Calculate monthly stats (last 30 days)
                var monthStart = today.AddDays(-30);
                var monthlyRevenue = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date >= monthStart)
                    .SumAsync(wh => wh.Price);
                
                var washesThisMonth = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && wh.WashDate.Date >= monthStart)
                    .CountAsync();
                
                var newCustomersThisMonth = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId && lc.CreatedAt.Date >= monthStart)
                    .CountAsync();

                var dashboardStats = new MerchantDashboardDto
                {
                    TotalCustomers = totalCustomers,
                    NewCustomersToday = newCustomersToday,
                    WashesToday = washesToday,
                    LastWashTime = lastWash != null ? GetTimeAgo(lastWash.WashDate) : "لا يوجد",
                    RewardsGiven = rewardsGiven,
                    PendingRewards = pendingRewards,
                    TodayRevenue = todayRevenue,
                    TotalRevenue = totalRevenue,
                    TotalWashesAllTime = totalWashesAllTime,
                    // Weekly stats
                    WeeklyRevenue = weeklyRevenue,
                    WashesThisWeek = washesThisWeek,
                    NewCustomersThisWeek = newCustomersThisWeek,
                    // Monthly stats
                    MonthlyRevenue = monthlyRevenue,
                    WashesThisMonth = washesThisMonth,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    Plan = merchant.Plan,
                    SubscriptionStatus = merchant.SubscriptionStatus,
                    PlanExpiryDate = merchant.PlanExpiryDate,
                    QRCodeImageUrl = merchant.QRCodeImageUrl,
                    RegistrationCode = merchant.RegistrationCode,
                    RecentActivity = new List<ActivityDto>()
                };

                // Get recent activity - include both washes and new customers
                var recentWashes = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId)
                    .OrderByDescending(wh => wh.WashDate)
                    .Take(5)
                    .Include(wh => wh.Customer)
                    .ThenInclude(c => c!.User)
                    .ToListAsync();

                var recentCustomers = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId)
                    .OrderByDescending(lc => lc.CreatedAt)
                    .Take(5)
                    .Include(lc => lc.Customer)
                    .ThenInclude(c => c!.User)
                    .ToListAsync();

                foreach (var wash in recentWashes)
                {
                    dashboardStats.RecentActivity.Add(new ActivityDto
                    {
                        Type = "wash",
                        Title = "غسلة جديدة",
                        Description = $"غسلة للعميل {wash.Customer?.User?.Name ?? "غير معروف"}",
                        Time = GetTimeAgo(wash.WashDate)
                    });
                }

                // Add new customer registrations
                foreach (var lc in recentCustomers)
                {
                    dashboardStats.RecentActivity.Add(new ActivityDto
                    {
                        Type = "customer",
                        Title = "عميل جديد",
                        Description = $"انضم العميل {lc.Customer?.User?.Name ?? "غير معروف"}",
                        Time = GetTimeAgo(lc.CreatedAt)
                    });
                }

                // Sort by time (most recent first) - we'll need to parse time ago for sorting, so sort before converting
                dashboardStats.RecentActivity = dashboardStats.RecentActivity.Take(10).ToList();

                return new ApiResponse<MerchantDashboardDto>
                {
                    Success = true,
                    Data = dashboardStats
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantDashboardDto>
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

        public async Task<ApiResponse<List<MerchantCustomerDto>>> GetCustomersAsync(string merchantId)
        {
            try
            {
                // First get the loyalty cards with customer info
                var loyaltyCards = await _context.LoyaltyCards
                    .Where(lc => lc.MerchantId == merchantId)
                    .Include(lc => lc.Customer)
                    .ThenInclude(c => c.User)
                    .ToListAsync();

                var customerIds = loyaltyCards.Select(lc => lc.CustomerId).Distinct().ToList();
                
                // Get wash counts per customer for this merchant
                var washCounts = await _context.WashHistories
                    .Where(wh => wh.MerchantId == merchantId && customerIds.Contains(wh.CustomerId))
                    .GroupBy(wh => wh.CustomerId)
                    .Select(g => new { 
                        CustomerId = g.Key, 
                        TotalWashes = g.Count(),
                        LastWash = g.Max(wh => wh.WashDate)
                    })
                    .ToListAsync();

                var washCountDict = washCounts.ToDictionary(x => x.CustomerId);

                var customers = loyaltyCards.Select(lc => new MerchantCustomerDto
                {
                    Id = lc.Customer.Id,
                    Name = lc.Customer.User.Name,
                    Phone = lc.Customer.User.Phone,
                    Email = lc.Customer.User.Email,
                    PlateNumber = lc.Customer.PlateNumber,
                    CarPhoto = lc.Customer.CarPhoto,
                    JoinDate = lc.CreatedAt,
                    CurrentWashes = lc.WashesCompleted,
                    TotalWashesRequired = lc.WashesRequired,
                    TotalWashes = washCountDict.ContainsKey(lc.CustomerId) ? washCountDict[lc.CustomerId].TotalWashes : 0,
                    LastWash = washCountDict.ContainsKey(lc.CustomerId) ? washCountDict[lc.CustomerId].LastWash : null,
                    DaysLeft = Math.Max(0, (int)(lc.ExpiresAt - DateTime.UtcNow).TotalDays),
                    Status = lc.IsActive ? "active" : "inactive"
                })
                .ToList();

                return new ApiResponse<List<MerchantCustomerDto>>
                {
                    Success = true,
                    Data = customers
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<MerchantCustomerDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<MerchantSettingsDto>> GetSettingsAsync(string merchantId)
        {
            try
            {
                var settings = await _context.MerchantSettings
                    .FirstOrDefaultAsync(ms => ms.MerchantId == merchantId);

                if (settings == null)
                {
                    // Create default settings if not exist
                    var merchant = await _context.Merchants.FindAsync(merchantId);
                    settings = new MerchantSettings
                    {
                        MerchantId = merchantId,
                        RewardWashesRequired = 5,
                        RewardTimeLimitDays = 30,
                        RewardDescription = "غسلة مجانية",
                        EnableCarPhoto = merchant?.Plan.ToLower() == "pro"
                    };
                    _context.MerchantSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                var dto = new MerchantSettingsDto
                {
                    Id = settings.Id,
                    RewardWashesRequired = settings.RewardWashesRequired,
                    RewardTimeLimitDays = settings.RewardTimeLimitDays,
                    RewardDescription = settings.RewardDescription,
                    RewardDescriptionAr = settings.RewardDescriptionAr,
                    AntiFraudSameDay = settings.AntiFraudSameDay,
                    EnableCarPhoto = settings.EnableCarPhoto,
                    IsLoyaltyPaused = settings.IsLoyaltyPaused,
                    LoyaltyPausedUntil = settings.LoyaltyPausedUntil,
                    NotificationsEnabled = settings.NotificationsEnabled,
                    NotificationTemplateWelcome = settings.NotificationTemplateWelcome,
                    NotificationTemplateRemaining = settings.NotificationTemplateRemaining,
                    NotificationTemplateRewardClose = settings.NotificationTemplateRewardClose,
                    CustomPrimaryColor = settings.CustomPrimaryColor,
                    CustomSecondaryColor = settings.CustomSecondaryColor,
                    CustomBusinessTagline = settings.CustomBusinessTagline
                };

                return new ApiResponse<MerchantSettingsDto>
                {
                    Success = true,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantSettingsDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateSettingsAsync(string merchantId, MerchantSettingsDto settingsDto)
        {
            try
            {
                var settings = await _context.MerchantSettings
                    .FirstOrDefaultAsync(ms => ms.MerchantId == merchantId);

                if (settings == null)
                    return new ApiResponse<bool> { Success = false, Message = "الإعدادات غير موجودة" };

                settings.RewardWashesRequired = settingsDto.RewardWashesRequired;
                settings.RewardTimeLimitDays = settingsDto.RewardTimeLimitDays;
                settings.RewardDescription = settingsDto.RewardDescription;
                settings.RewardDescriptionAr = settingsDto.RewardDescriptionAr;
                settings.AntiFraudSameDay = settingsDto.AntiFraudSameDay;
                settings.EnableCarPhoto = settingsDto.EnableCarPhoto;
                settings.IsLoyaltyPaused = settingsDto.IsLoyaltyPaused;
                settings.LoyaltyPausedUntil = settingsDto.LoyaltyPausedUntil;
                settings.NotificationsEnabled = settingsDto.NotificationsEnabled;
                settings.NotificationTemplateWelcome = settingsDto.NotificationTemplateWelcome;
                settings.NotificationTemplateRemaining = settingsDto.NotificationTemplateRemaining;
                settings.NotificationTemplateRewardClose = settingsDto.NotificationTemplateRewardClose;
                settings.CustomPrimaryColor = settingsDto.CustomPrimaryColor;
                settings.CustomSecondaryColor = settingsDto.CustomSecondaryColor;
                settings.CustomBusinessTagline = settingsDto.CustomBusinessTagline;
                settings.UpdatedAt = DateTime.UtcNow;

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

        public async Task<ApiResponse<QRScanResultDto>> ProcessQRScanAsync(ProcessWashRequest request)
        {
            try
            {
                // Check merchant subscription status first
                var merchant = await _context.Merchants.FindAsync(request.MerchantId);
                if (merchant == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "التاجر غير موجود"
                    };

                // Check if subscription is active - if not, cannot scan washes
                if (merchant.SubscriptionStatus != "active")
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "الاشتراك غير فعال. يرجى تجديد الاشتراك للمتابعة."
                    };

                // Check if subscription expired
                if (merchant.PlanExpiryDate != DateTime.MinValue && merchant.PlanExpiryDate < DateTime.UtcNow)
                {
                    merchant.SubscriptionStatus = "expired";
                    await _context.SaveChangesAsync();
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "انتهى الاشتراك. يرجى تجديد الاشتراك للمتابعة."
                    };
                }

                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.QRCode == request.CustomerQRCode);

                if (customer == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "QR Code غير صحيح"
                    };

                var loyaltyCard = await _context.LoyaltyCards
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customer.Id && lc.MerchantId == request.MerchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "العميل ليس مشتركاً في هذه المغسلة"
                    };

                var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == request.MerchantId);

                // Check if loyalty is paused
                if (settings?.IsLoyaltyPaused == true)
                {
                    if (settings.LoyaltyPausedUntil == null || settings.LoyaltyPausedUntil > DateTime.UtcNow)
                    {
                        return new ApiResponse<QRScanResultDto>
                        {
                            Success = false,
                            Message = "برنامج الولاء معلق مؤقتاً"
                        };
                    }
                    else
                    {
                        // Auto-unpause if pause period ended
                        settings.IsLoyaltyPaused = false;
                        settings.LoyaltyPausedUntil = null;
                    }
                }

                // Anti-fraud: check same day wash
                if (settings?.AntiFraudSameDay == true)
                {
                    var todayWash = await _context.WashHistories
                        .AnyAsync(wh => wh.CustomerId == customer.Id 
                            && wh.MerchantId == request.MerchantId 
                            && wh.WashDate.Date == DateTime.UtcNow.Date);
                    
                    if (todayWash)
                        return new ApiResponse<QRScanResultDto>
                        {
                            Success = false,
                            Message = "تم تسجيل غسلة لهذا العميل اليوم بالفعل"
                        };
                }

                // Create wash history
                var wash = new WashHistory
                {
                    CustomerId = customer.Id,
                    MerchantId = request.MerchantId,
                    WashDate = DateTime.UtcNow,
                    ServiceDescription = request.ServiceDescription,
                    Price = request.Price,
                    Status = "completed"
                };

                // Update merchant stats
                merchant.TotalWashes++;

                // Update loyalty card
                loyaltyCard.WashesCompleted++;
                customer.TotalWashes++;
                
                // Update customer total spent
                customer.TotalSpent += request.Price;
                customer.LastWashAt = DateTime.UtcNow;

                var rewardEarned = false;
                var washesRemaining = loyaltyCard.WashesRequired - loyaltyCard.WashesCompleted;

                // Create notification for wash completion
                await CreateNotificationAsync(customer.Id, "تم تسجيل غسلة جديدة", 
                    $"تم تسجيل غسلة لدى {merchant.BusinessName}. باقي لك {Math.Max(0, washesRemaining)} غسلات للحصول على المكافأة.",
                    "success");

                // Check if close to reward (2 washes remaining)
                if (washesRemaining > 0 && washesRemaining <= 2)
                {
                    await CreateNotificationAsync(customer.Id, "أنت قريب من المكافأة! 🎉",
                        $"باقي لك {washesRemaining} غسلات فقط للحصول على مكافأة من {merchant.BusinessName}",
                        "info");
                }

                // Check if reward earned
                if (loyaltyCard.WashesCompleted >= loyaltyCard.WashesRequired)
                {
                    rewardEarned = true;
                    
                    // Generate reward QR code
                    var rewardQRCode = $"RWD-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                    
                    // Mark reward as earned on loyalty card
                    loyaltyCard.IsRewardEarned = true;
                    loyaltyCard.RewardEarnedAt = DateTime.UtcNow;
                    loyaltyCard.RewardQRCode = rewardQRCode;

                    // Create a Reward record in the Rewards table
                    var reward = new Reward
                    {
                        CustomerId = customer.Id,
                        MerchantId = request.MerchantId,
                        LoyaltyCardId = loyaltyCard.Id,
                        ExpiresAt = DateTime.UtcNow.AddDays(settings?.RewardTimeLimitDays ?? 30),
                        Status = "available",
                        RewardQRCode = rewardQRCode,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Rewards.Add(reward);
                    
                    // Update customer rewards earned counter
                    customer.TotalRewardsEarned++;

                    // Create notification for reward earned with full reward description
                    var rewardDesc = settings?.RewardDescription ?? "مكافأة";
                    await CreateNotificationAsync(customer.Id, "🎉 مبروك! حصلت على مكافأة",
                        $"لقد حصلت على: {rewardDesc} من {merchant.BusinessName}. يمكنك استلام مكافأتك من المغسلة.",
                        "success");
                    
                    // Reset loyalty card for next cycle - customer can start earning new reward immediately
                    loyaltyCard.WashesCompleted = 0;
                    loyaltyCard.IsRewardEarned = false; // Allow new cycle to start
                    loyaltyCard.RewardQRCode = null; // QR code is stored in Reward record
                    loyaltyCard.ExpiresAt = DateTime.UtcNow.AddDays(settings?.RewardTimeLimitDays ?? 30);
                }

                _context.WashHistories.Add(wash);
                await _context.SaveChangesAsync();

                // Get car photo from CarPhotos table if exists
                var carPhoto = await _context.CarPhotos
                    .Where(cp => cp.CustomerId == customer.Id)
                    .OrderByDescending(cp => cp.UploadedAt)
                    .Select(cp => cp.PhotoUrl)
                    .FirstOrDefaultAsync();

                var result = new QRScanResultDto
                {
                    Success = true,
                    Title = rewardEarned ? "🎉 مبروك! العميل حصل على المكافأة" : "تم تسجيل الغسلة بنجاح",
                    CustomerName = customer.User?.Name ?? "",
                    CustomerPhone = customer.User?.Phone ?? "",
                    CustomerPhoto = carPhoto ?? customer.CarPhoto, // Use CarPhotos table first, fallback to Customer.CarPhoto
                    PlateNumber = customer.PlateNumber,
                    CurrentWashes = loyaltyCard.WashesCompleted,
                    TotalWashesRequired = loyaltyCard.WashesRequired,
                    Progress = loyaltyCard.WashesRequired > 0 ? (decimal)loyaltyCard.WashesCompleted / loyaltyCard.WashesRequired * 100 : 0,
                    DaysLeft = Math.Max(0, (int)(loyaltyCard.ExpiresAt - DateTime.UtcNow).TotalDays),
                    RewardEarned = rewardEarned,
                    RewardDescription = settings?.RewardDescription
                };

                return new ApiResponse<QRScanResultDto>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<QRScanResultDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateLoyaltyCardAsync(string merchantId, string customerId)
        {
            try
            {
                var merchant = await _context.Merchants.FindAsync(merchantId);
                var customer = await _context.Customers.FindAsync(customerId);

                if (merchant == null || customer == null)
                    return new ApiResponse<bool> { Success = false };

                var settings = await _context.MerchantSettings
                    .FirstOrDefaultAsync(ms => ms.MerchantId == merchantId);

                var card = new LoyaltyCard
                {
                    CustomerId = customerId,
                    MerchantId = merchantId,
                    WashesRequired = settings?.RewardWashesRequired ?? 5,
                    ExpiresAt = DateTime.UtcNow.AddDays(settings?.RewardTimeLimitDays ?? 30)
                };

                _context.LoyaltyCards.Add(card);
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

        public async Task<ApiResponse<bool>> UpdatePasswordAsync(string merchantId, string currentPassword, string newPassword)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false };

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, merchant.User.PasswordHash))
                    return new ApiResponse<bool> { Success = false, Message = "كلمة المرور الحالية غير صحيحة" };

                merchant.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
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

        public async Task<ApiResponse<bool>> UpdateProfileAsync(string merchantId, MerchantProfileDto profileDto)
        {
            try
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == merchantId);

                if (merchant == null)
                    return new ApiResponse<bool> { Success = false };

                merchant.BusinessName = profileDto.BusinessName;
                merchant.City = profileDto.City;
                merchant.User.Phone = profileDto.Phone;
                merchant.UpdatedAt = DateTime.UtcNow;

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

        /// <summary>
        /// Create a notification for a customer
        /// </summary>
        private async Task CreateNotificationAsync(string customerId, string title, string message, string type)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                Title = title,
                Message = message,
                Type = type, // info, success, warning, promotion
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            // Note: SaveChangesAsync is called by the parent method
        }

        /// <summary>
        /// Upload merchant logo
        /// </summary>
        public async Task<ApiResponse<string>> UploadLogoAsync(string merchantId, string logoUrl)
        {
            try
            {
                var merchant = await _context.Merchants.FindAsync(merchantId);
                if (merchant == null)
                    return new ApiResponse<string> { Success = false, Message = "التاجر غير موجود" };

                // Logos removed from system - this endpoint is deprecated
                merchant.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponse<string> { Success = true, Data = logoUrl, Message = "تم رفع الشعار بنجاح" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        /// <summary>
        /// Validate a reward QR code
        /// </summary>
        public async Task<ApiResponse<RewardValidationDto>> ValidateRewardQRCodeAsync(string merchantId, string rewardQRCode)
        {
            try
            {
                // Find the reward in the Rewards table (not LoyaltyCards)
                var reward = await _context.Rewards
                    .Include(r => r.Customer)
                    .ThenInclude(c => c!.User)
                    .Include(r => r.Merchant)
                    .FirstOrDefaultAsync(r => r.RewardQRCode == rewardQRCode && r.MerchantId == merchantId);

                if (reward == null)
                {
                    // Also check if it's in LoyaltyCards (legacy support)
                    var loyaltyCard = await _context.LoyaltyCards
                        .Include(lc => lc.Customer)
                        .ThenInclude(c => c!.User)
                        .FirstOrDefaultAsync(lc => lc.RewardQRCode == rewardQRCode && lc.MerchantId == merchantId);
                    
                    if (loyaltyCard == null)
                    {
                        return new ApiResponse<RewardValidationDto>
                        {
                            Success = false,
                            Message = "رمز المكافأة غير صالح أو لا ينتمي لهذه المغسلة",
                            Data = new RewardValidationDto { Success = false, Title = "رمز غير صالح" }
                        };
                    }
                    
                    // Legacy path - use LoyaltyCard data
                    var settingsLegacy = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == merchantId);
                    var resultLegacy = new RewardValidationDto
                    {
                        Success = !loyaltyCard.IsRewardClaimed && loyaltyCard.IsRewardEarned,
                        Title = loyaltyCard.IsRewardClaimed ? "مكافأة مستخدمة" : (loyaltyCard.IsRewardEarned ? "مكافأة صالحة" : "المكافأة غير مكتملة"),
                        Message = loyaltyCard.IsRewardClaimed ? "تم استخدام هذه المكافأة مسبقاً" : "",
                        CustomerName = loyaltyCard.Customer?.User?.Name ?? "غير معروف",
                        CustomerPhone = loyaltyCard.Customer?.User?.Phone ?? "-",
                        RewardTitle = settingsLegacy?.RewardDescription ?? "غسلة مجانية",
                        RewardType = "free_wash",
                        RewardValue = 0,
                        RewardExpiresAt = loyaltyCard.ExpiresAt,
                        IsAlreadyClaimed = loyaltyCard.IsRewardClaimed,
                        IsExpired = loyaltyCard.ExpiresAt < DateTime.UtcNow
                    };
                    
                    return new ApiResponse<RewardValidationDto>
                    {
                        Success = true,
                        Data = resultLegacy
                    };
                }

                // Reward found in Rewards table
                var isExpired = reward.ExpiresAt < DateTime.UtcNow;
                var isClaimed = reward.Status == "claimed";

                // Get merchant settings for reward description
                var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == merchantId);
                
                var result = new RewardValidationDto
                {
                    Success = !isClaimed && !isExpired,
                    Title = isClaimed ? "مكافأة مستخدمة" : (isExpired ? "مكافأة منتهية" : "مكافأة صالحة"),
                    Message = isClaimed ? "تم استخدام هذه المكافأة مسبقاً" : (isExpired ? "انتهت صلاحية المكافأة" : ""),
                    CustomerName = reward.Customer?.User?.Name ?? "غير معروف",
                    CustomerPhone = reward.Customer?.User?.Phone ?? "-",
                    RewardTitle = settings?.RewardDescription ?? "غسلة مجانية",
                    RewardType = "free_wash",
                    RewardValue = 0,
                    RewardExpiresAt = reward.ExpiresAt,
                    IsAlreadyClaimed = isClaimed,
                    IsExpired = isExpired
                };

                return new ApiResponse<RewardValidationDto>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<RewardValidationDto>
                {
                    Success = false,
                    Message = "حدث خطأ",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Redeem a reward
        /// </summary>
        public async Task<ApiResponse<bool>> RedeemRewardAsync(string merchantId, string rewardQRCode)
        {
            try
            {
                // First check the Rewards table (new system)
                var reward = await _context.Rewards
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.RewardQRCode == rewardQRCode && r.MerchantId == merchantId);

                if (reward != null)
                {
                    // Use new Rewards table
                    if (reward.Status == "claimed")
                    {
                        return new ApiResponse<bool> { Success = false, Message = "تم استخدام هذه المكافأة مسبقاً" };
                    }

                    if (reward.ExpiresAt < DateTime.UtcNow)
                    {
                        reward.Status = "expired";
                        await _context.SaveChangesAsync();
                        return new ApiResponse<bool> { Success = false, Message = "انتهت صلاحية المكافأة" };
                    }

                    // Mark reward as claimed
                    reward.Status = "claimed";
                    reward.ClaimedAt = DateTime.UtcNow;
                    reward.ClaimedByMerchantId = merchantId;
                    reward.UpdatedAt = DateTime.UtcNow;

                    // Also update customer's rewards claimed counter
                    if (reward.Customer != null)
                    {
                        reward.Customer.TotalRewardsClaimed++;
                    }

                    // Also update the associated LoyaltyCard if it exists
                    var loyaltyCard = await _context.LoyaltyCards
                        .FirstOrDefaultAsync(lc => lc.Id == reward.LoyaltyCardId);
                    
                    if (loyaltyCard != null)
                    {
                        loyaltyCard.IsRewardClaimed = true;
                        loyaltyCard.RewardClaimedAt = DateTime.UtcNow;
                        // Reset the loyalty card for a new cycle
                        loyaltyCard.WashesCompleted = 0;
                        loyaltyCard.IsRewardEarned = false;
                        loyaltyCard.RewardQRCode = null;
                        loyaltyCard.RewardEarnedAt = null;
                        loyaltyCard.ExpiresAt = DateTime.UtcNow.AddDays(30);
                    }

                    // Create notification for customer
                    await CreateNotificationAsync(
                        reward.CustomerId,
                        "🎉 تم استخدام المكافأة",
                        "تم استخدام مكافأتك بنجاح! ابدأ رحلة جديدة للحصول على المكافأة التالية.",
                        "success"
                    );

                    await _context.SaveChangesAsync();

                    return new ApiResponse<bool> { Success = true, Data = true, Message = "تم استخدام المكافأة بنجاح" };
                }

                // Fallback: Check LoyaltyCards table (legacy)
                var legacyCard = await _context.LoyaltyCards
                    .Include(lc => lc.Customer)
                    .FirstOrDefaultAsync(lc => lc.RewardQRCode == rewardQRCode && lc.MerchantId == merchantId);

                if (legacyCard == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "رمز المكافأة غير صالح" };
                }

                if (legacyCard.IsRewardClaimed)
                {
                    return new ApiResponse<bool> { Success = false, Message = "تم استخدام هذه المكافأة مسبقاً" };
                }

                if (!legacyCard.IsRewardEarned)
                {
                    return new ApiResponse<bool> { Success = false, Message = "لم يتم اكتمال المكافأة بعد" };
                }

                if (legacyCard.ExpiresAt < DateTime.UtcNow)
                {
                    return new ApiResponse<bool> { Success = false, Message = "انتهت صلاحية المكافأة" };
                }

                // Mark reward as claimed
                legacyCard.IsRewardClaimed = true;
                legacyCard.RewardClaimedAt = DateTime.UtcNow;

                // Reset the loyalty card for a new cycle
                legacyCard.WashesCompleted = 0;
                legacyCard.IsRewardEarned = false;
                legacyCard.RewardQRCode = null;
                legacyCard.RewardEarnedAt = null;
                legacyCard.ExpiresAt = DateTime.UtcNow.AddDays(30); // Reset expiry

                // Create notification for customer
                await CreateNotificationAsync(
                    legacyCard.CustomerId,
                    "🎉 تم استخدام المكافأة",
                    "تم استخدام مكافأتك بنجاح! ابدأ رحلة جديدة للحصول على المكافأة التالية.",
                    "success"
                );

                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم استخدام المكافأة بنجاح" };
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

        /// <summary>
        /// Get customer info by QR code without recording a wash (validation only)
        /// </summary>
        public async Task<ApiResponse<QRScanResultDto>> GetCustomerByQRCodeAsync(string merchantId, string customerQRCode)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.QRCode == customerQRCode);

                if (customer == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "QR Code غير صحيح"
                    };

                var loyaltyCard = await _context.LoyaltyCards
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customer.Id && lc.MerchantId == merchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "العميل ليس مشتركاً في هذه المغسلة",
                        Data = new QRScanResultDto
                        {
                            Success = false,
                            CustomerName = customer.User?.Name ?? "غير معروف",
                            CustomerPhone = customer.User?.Phone ?? "-",
                            PlateNumber = customer.PlateNumber
                        }
                    };

                var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == merchantId);

                // Get car photo
                var carPhoto = await _context.CarPhotos
                    .Where(cp => cp.CustomerId == customer.Id)
                    .OrderByDescending(cp => cp.UploadedAt)
                    .Select(cp => cp.PhotoUrl)
                    .FirstOrDefaultAsync();

                var result = new QRScanResultDto
                {
                    Success = true,
                    Title = "تم التعرف على العميل",
                    CustomerName = customer.User?.Name ?? "",
                    CustomerPhone = customer.User?.Phone ?? "",
                    CustomerPhoto = carPhoto ?? customer.CarPhoto,
                    PlateNumber = customer.PlateNumber,
                    CurrentWashes = loyaltyCard.WashesCompleted,
                    TotalWashesRequired = loyaltyCard.WashesRequired,
                    Progress = loyaltyCard.WashesRequired > 0 ? (decimal)loyaltyCard.WashesCompleted / loyaltyCard.WashesRequired * 100 : 0,
                    DaysLeft = Math.Max(0, (int)(loyaltyCard.ExpiresAt - DateTime.UtcNow).TotalDays),
                    RewardEarned = loyaltyCard.IsRewardEarned,
                    RewardDescription = settings?.RewardDescription
                };

                return new ApiResponse<QRScanResultDto>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<QRScanResultDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Record a wash for a customer (validation already done via GetCustomerByQRCodeAsync)
        /// This is the actual wash recording - separated from validation to prevent double washes
        /// </summary>
        public async Task<ApiResponse<QRScanResultDto>> RecordWashAsync(ProcessWashRequest request)
        {
            try
            {
                // Check merchant subscription status first
                var merchant = await _context.Merchants.FindAsync(request.MerchantId);
                if (merchant == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "التاجر غير موجود"
                    };

                // Check if subscription is active - if not, cannot scan washes
                if (merchant.SubscriptionStatus != "active")
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "الاشتراك غير فعال. يرجى تجديد الاشتراك للمتابعة."
                    };

                // Check if subscription expired
                if (merchant.PlanExpiryDate != DateTime.MinValue && merchant.PlanExpiryDate < DateTime.UtcNow)
                {
                    merchant.SubscriptionStatus = "expired";
                    await _context.SaveChangesAsync();
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "انتهى الاشتراك. يرجى تجديد الاشتراك للمتابعة."
                    };
                }

                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.QRCode == request.CustomerQRCode);

                if (customer == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "QR Code غير صحيح"
                    };

                var loyaltyCard = await _context.LoyaltyCards
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customer.Id && lc.MerchantId == request.MerchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<QRScanResultDto>
                    {
                        Success = false,
                        Message = "العميل ليس مشتركاً في هذه المغسلة"
                    };

                var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == request.MerchantId);

                // Check if loyalty is paused
                if (settings?.IsLoyaltyPaused == true)
                {
                    if (settings.LoyaltyPausedUntil == null || settings.LoyaltyPausedUntil > DateTime.UtcNow)
                    {
                        return new ApiResponse<QRScanResultDto>
                        {
                            Success = false,
                            Message = "برنامج الولاء معلق مؤقتاً"
                        };
                    }
                    else
                    {
                        // Auto-unpause if pause period ended
                        settings.IsLoyaltyPaused = false;
                        settings.LoyaltyPausedUntil = null;
                    }
                }

                // Anti-fraud: check same day wash
                if (settings?.AntiFraudSameDay == true)
                {
                    var todayWash = await _context.WashHistories
                        .AnyAsync(wh => wh.CustomerId == customer.Id 
                            && wh.MerchantId == request.MerchantId 
                            && wh.WashDate.Date == DateTime.UtcNow.Date);
                    
                    if (todayWash)
                        return new ApiResponse<QRScanResultDto>
                        {
                            Success = false,
                            Message = "تم تسجيل غسلة لهذا العميل اليوم بالفعل"
                        };
                }

                // Create wash history
                var wash = new WashHistory
                {
                    CustomerId = customer.Id,
                    MerchantId = request.MerchantId,
                    WashDate = DateTime.UtcNow,
                    ServiceDescription = request.ServiceDescription,
                    Price = request.Price,
                    Status = "completed"
                };

                // Update merchant stats
                merchant.TotalWashes++;

                // Update loyalty card
                loyaltyCard.WashesCompleted++;
                customer.TotalWashes++;
                
                // Update customer total spent
                customer.TotalSpent += request.Price;
                customer.LastWashAt = DateTime.UtcNow;

                var rewardEarned = false;
                var washesRemaining = loyaltyCard.WashesRequired - loyaltyCard.WashesCompleted;

                // Create notification for wash completion
                await CreateNotificationAsync(customer.Id, "تم تسجيل غسلة جديدة", 
                    $"تم تسجيل غسلة لدى {merchant.BusinessName}. باقي لك {Math.Max(0, washesRemaining)} غسلات للحصول على المكافأة.",
                    "success");

                // Check if close to reward (2 washes remaining)
                if (washesRemaining > 0 && washesRemaining <= 2)
                {
                    await CreateNotificationAsync(customer.Id, "أنت قريب من المكافأة! 🎉",
                        $"باقي لك {washesRemaining} غسلات فقط للحصول على مكافأة من {merchant.BusinessName}",
                        "info");
                }

                // Check if reward earned
                if (loyaltyCard.WashesCompleted >= loyaltyCard.WashesRequired)
                {
                    rewardEarned = true;
                    
                    // Generate reward QR code
                    var rewardQRCode = $"RWD-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                    
                    // Mark reward as earned on loyalty card
                    loyaltyCard.IsRewardEarned = true;
                    loyaltyCard.RewardEarnedAt = DateTime.UtcNow;
                    loyaltyCard.RewardQRCode = rewardQRCode;

                    // Create a Reward record in the Rewards table
                    var reward = new Reward
                    {
                        CustomerId = customer.Id,
                        MerchantId = request.MerchantId,
                        LoyaltyCardId = loyaltyCard.Id,
                        ExpiresAt = DateTime.UtcNow.AddDays(settings?.RewardTimeLimitDays ?? 30),
                        Status = "available",
                        RewardQRCode = rewardQRCode,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Rewards.Add(reward);
                    
                    // Update customer rewards earned counter
                    customer.TotalRewardsEarned++;

                    // Create notification for reward earned with full reward description
                    var rewardDesc = settings?.RewardDescription ?? "مكافأة";
                    await CreateNotificationAsync(customer.Id, "🎉 مبروك! حصلت على مكافأة",
                        $"لقد حصلت على: {rewardDesc} من {merchant.BusinessName}. يمكنك استلام مكافأتك من المغسلة.",
                        "success");
                    
                    // Reset loyalty card for next cycle - customer can start earning new reward immediately
                    loyaltyCard.WashesCompleted = 0;
                    loyaltyCard.IsRewardEarned = false; // Allow new cycle to start
                    loyaltyCard.RewardQRCode = null; // QR code is stored in Reward record
                    loyaltyCard.ExpiresAt = DateTime.UtcNow.AddDays(settings?.RewardTimeLimitDays ?? 30);
                }

                _context.WashHistories.Add(wash);
                await _context.SaveChangesAsync();

                // Get car photo from CarPhotos table if exists
                var carPhoto = await _context.CarPhotos
                    .Where(cp => cp.CustomerId == customer.Id)
                    .OrderByDescending(cp => cp.UploadedAt)
                    .Select(cp => cp.PhotoUrl)
                    .FirstOrDefaultAsync();

                var result = new QRScanResultDto
                {
                    Success = true,
                    Title = rewardEarned ? "🎉 مبروك! العميل استحق مكافأة" : "تم تسجيل الغسلة بنجاح",
                    CustomerName = customer.User?.Name ?? "",
                    CustomerPhone = customer.User?.Phone ?? "",
                    CustomerPhoto = carPhoto ?? customer.CarPhoto,
                    PlateNumber = customer.PlateNumber,
                    CurrentWashes = loyaltyCard.WashesCompleted,
                    TotalWashesRequired = loyaltyCard.WashesRequired,
                    Progress = loyaltyCard.WashesRequired > 0 ? (decimal)loyaltyCard.WashesCompleted / loyaltyCard.WashesRequired * 100 : 0,
                    DaysLeft = Math.Max(0, (int)(loyaltyCard.ExpiresAt - DateTime.UtcNow).TotalDays),
                    RewardEarned = rewardEarned,
                    RewardDescription = settings?.RewardDescription
                };

                return new ApiResponse<QRScanResultDto>
                {
                    Success = true,
                    Message = rewardEarned ? "تم تسجيل الغسلة والعميل استحق مكافأة!" : "تم تسجيل الغسلة بنجاح",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<QRScanResultDto>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Update customer information
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateCustomerAsync(string merchantId, string customerId, MerchantCustomerUpdateDto customerData)
        {
            try
            {
                // Find the loyalty card to verify customer belongs to this merchant
                var loyaltyCard = await _context.LoyaltyCards
                    .Include(lc => lc.Customer)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customerId && lc.MerchantId == merchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<bool> { Success = false, Message = "العميل غير موجود" };

                var user = loyaltyCard.Customer.User;
                user.Name = customerData.Name;
                user.Email = customerData.Email;
                user.Phone = customerData.Phone;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم تحديث بيانات العميل بنجاح" };
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

        /// <summary>
        /// Delete customer (removes loyalty card, not the customer account)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteCustomerAsync(string merchantId, string customerId)
        {
            try
            {
                var loyaltyCard = await _context.LoyaltyCards
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customerId && lc.MerchantId == merchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<bool> { Success = false, Message = "العميل غير موجود" };

                // Remove loyalty card (customer can still exist for other merchants)
                _context.LoyaltyCards.Remove(loyaltyCard);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم حذف العميل بنجاح" };
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

        /// <summary>
        /// Toggle customer status (activate/deactivate)
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleCustomerStatusAsync(string merchantId, string customerId, bool activate)
        {
            try
            {
                var loyaltyCard = await _context.LoyaltyCards
                    .FirstOrDefaultAsync(lc => lc.CustomerId == customerId && lc.MerchantId == merchantId);

                if (loyaltyCard == null)
                    return new ApiResponse<bool> { Success = false, Message = "العميل غير موجود" };

                loyaltyCard.IsActive = activate;
                loyaltyCard.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var message = activate ? "تم تفعيل العميل بنجاح" : "تم تعطيل العميل بنجاح";
                return new ApiResponse<bool> { Success = true, Data = true, Message = message };
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
