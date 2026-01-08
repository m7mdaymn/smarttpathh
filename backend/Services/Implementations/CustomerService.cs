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
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CustomerProfileDto>> GetCustomerProfileAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.LoyaltyCards)
                        .ThenInclude(lc => lc.Merchant)
                    .Include(c => c.WashHistories)
                    .Include(c => c.Notifications)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new ApiResponse<CustomerProfileDto>
                    {
                        Success = false,
                        Message = "العميل غير موجود"
                    };

                var loyaltyCards = new List<LoyaltyCardDto>();
                foreach (var lc in customer.LoyaltyCards)
                {
                    var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == lc.MerchantId);
                    loyaltyCards.Add(new LoyaltyCardDto
                    {
                        Id = lc.Id,
                        MerchantId = lc.MerchantId,
                        MerchantName = lc.Merchant?.BusinessName ?? "",
                        MerchantCity = lc.Merchant?.City,
                        MerchantPhone = lc.Merchant?.User?.Phone,
                        WashesCompleted = lc.WashesCompleted,
                        WashesRequired = lc.WashesRequired,
                        ExpiresAt = lc.ExpiresAt,
                        IsActive = lc.IsActive,
                        IsPaused = lc.IsPaused,
                        Progress = lc.WashesRequired > 0 ? (decimal)lc.WashesCompleted / lc.WashesRequired * 100 : 0,
                        AllowCarPhotoUpload = (lc.Merchant?.Plan?.ToLower() == "pro") && (settings?.EnableCarPhoto == true),
                        RewardQRCode = lc.RewardQRCode,
                        IsRewardEarned = lc.IsRewardEarned,
                        RewardEarnedAt = lc.RewardEarnedAt,
                        IsRewardClaimed = lc.IsRewardClaimed,
                        RewardClaimedAt = lc.RewardClaimedAt,
                        // Merchant branding
                        CustomPrimaryColor = settings?.CustomPrimaryColor ?? "#3B82F6",
                        RewardDescription = settings?.RewardDescription ?? "غسلة مجانية",
                        RewardTimeLimitDays = settings?.RewardTimeLimitDays ?? 30,
                        // Statistics
                        DaysRemaining = Math.Max(0, (int)(lc.ExpiresAt - DateTime.UtcNow).TotalDays),
                        TotalWashesWithMerchant = await _context.WashHistories.CountAsync(wh => wh.CustomerId == customer.Id && wh.MerchantId == lc.MerchantId),
                        LastWashDate = await _context.WashHistories.Where(wh => wh.CustomerId == customer.Id && wh.MerchantId == lc.MerchantId).OrderByDescending(wh => wh.WashDate).Select(wh => (DateTime?)wh.WashDate).FirstOrDefaultAsync()
                    });
                }

                var profile = new CustomerProfileDto
                {
                    Id = customer.Id,
                    Name = customer.User?.Name ?? "",
                    Email = customer.User?.Email ?? "",
                    Phone = customer.User?.Phone ?? "",
                    QRCode = customer.QRCode,
                    PlateNumber = customer.PlateNumber,
                    CarPhoto = customer.CarPhoto,
                    TotalWashes = customer.TotalWashes,
                    LoyaltyCards = loyaltyCards,
                    Washes = customer.WashHistories.Select(wh => new WashHistoryDto
                    {
                        Id = wh.Id,
                        MerchantName = _context.Merchants.FirstOrDefault(m => m.Id == wh.MerchantId)?.BusinessName ?? "",
                        WashDate = wh.WashDate,
                        ServiceDescription = wh.ServiceDescription,
                        Price = wh.Price,
                        Status = wh.Status,
                        Rating = wh.Rating,
                        CustomerComment = wh.CustomerComment
                    }).ToList(),
                    Notifications = customer.Notifications.Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    }).ToList()
                };

                return new ApiResponse<CustomerProfileDto>
                {
                    Success = true,
                    Message = "تم جلب الملف الشخصي بنجاح",
                    Data = profile
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerProfileDto>
                {
                    Success = false,
                    Message = "حدث خطأ",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<LoyaltyCardDto>>> GetLoyaltyCardsAsync(string customerId)
        {
            try
            {
                var cards = await _context.LoyaltyCards
                    .Where(lc => lc.CustomerId == customerId && lc.IsActive)
                    .Include(lc => lc.Merchant)
                    .ToListAsync();

                var result = new List<LoyaltyCardDto>();
                foreach (var lc in cards)
                {
                    var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == lc.MerchantId);
                    result.Add(new LoyaltyCardDto
                    {
                        Id = lc.Id,
                        MerchantId = lc.MerchantId,
                        MerchantName = lc.Merchant?.BusinessName ?? "",
                        MerchantCity = lc.Merchant?.City,
                        MerchantPhone = lc.Merchant?.User?.Phone,
                        WashesCompleted = lc.WashesCompleted,
                        WashesRequired = lc.WashesRequired,
                        ExpiresAt = lc.ExpiresAt,
                        IsActive = lc.IsActive,
                        IsPaused = lc.IsPaused,
                        Progress = lc.WashesRequired > 0 ? (decimal)lc.WashesCompleted / lc.WashesRequired * 100 : 0,
                        AllowCarPhotoUpload = (lc.Merchant?.Plan?.ToLower() == "pro") && (settings?.EnableCarPhoto == true),
                        RewardQRCode = lc.RewardQRCode,
                        IsRewardEarned = lc.WashesCompleted >= lc.WashesRequired || !string.IsNullOrEmpty(lc.RewardQRCode),
                        IsRewardClaimed = lc.IsRewardClaimed,
                        RewardClaimedAt = lc.RewardClaimedAt,
                        // Merchant branding
                        CustomPrimaryColor = settings?.CustomPrimaryColor ?? "#3B82F6",
                        RewardDescription = settings?.RewardDescription ?? "غسلة مجانية",
                        RewardTimeLimitDays = settings?.RewardTimeLimitDays ?? 30,
                        // Statistics
                        DaysRemaining = Math.Max(0, (int)(lc.ExpiresAt - DateTime.UtcNow).TotalDays),
                        TotalWashesWithMerchant = await _context.WashHistories.CountAsync(wh => wh.CustomerId == lc.CustomerId && wh.MerchantId == lc.MerchantId),
                        LastWashDate = await _context.WashHistories.Where(wh => wh.CustomerId == lc.CustomerId && wh.MerchantId == lc.MerchantId).OrderByDescending(wh => wh.WashDate).Select(wh => (DateTime?)wh.WashDate).FirstOrDefaultAsync()
                    });
                }

                return new ApiResponse<List<LoyaltyCardDto>>
                {
                    Success = true,
                    Message = "تم جلب بطاقات الولاء",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LoyaltyCardDto>>
                {
                    Success = false,
                    Message = "حدث خطأ",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<WashHistoryDto>>> GetWashHistoryAsync(string customerId)
        {
            try
            {
                var washes = await _context.WashHistories
                    .Where(wh => wh.CustomerId == customerId)
                    .Include(wh => wh.Merchant)
                    .OrderByDescending(wh => wh.WashDate)
                    .ToListAsync();

                var result = washes.Select(wh => new WashHistoryDto
                {
                    Id = wh.Id,
                    MerchantName = wh.Merchant?.BusinessName ?? "",
                    WashDate = wh.WashDate,
                    Status = wh.Status,
                    Rating = wh.Rating,
                    ServiceDescription = wh.ServiceDescription ?? "",
                    Price = wh.Price,
                    CustomerComment = wh.CustomerComment
                }).ToList();

                return new ApiResponse<List<WashHistoryDto>>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<WashHistoryDto>>
                {
                    Success = false,
                    Message = "حدث خطأ",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(string customerId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.CustomerId == customerId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var result = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

                return new ApiResponse<List<NotificationDto>>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificationDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<RewardDto>>> GetRewardsAsync(string customerId)
        {
            try
            {
                var rewards = await _context.Rewards
                    .Where(r => r.CustomerId == customerId)
                    .Include(r => r.Merchant)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var result = new List<RewardDto>();
                
                foreach (var r in rewards)
                {
                    // Get merchant settings for reward description
                    var settings = await _context.MerchantSettings.FirstOrDefaultAsync(s => s.MerchantId == r.MerchantId);
                    var daysUntilExpiry = Math.Max(0, (int)(r.ExpiresAt - now).TotalDays);
                    var isExpired = r.ExpiresAt < now;
                    var status = isExpired ? "expired" : r.Status;
                    
                    result.Add(new RewardDto
                    {
                        Id = r.Id,
                        MerchantId = r.MerchantId,
                        MerchantName = r.Merchant?.BusinessName ?? "",
                        RewardDescription = settings?.RewardDescription ?? "غسلة مجانية",
                        ExpiresAt = r.ExpiresAt,
                        Status = status,
                        RewardQRCode = r.RewardQRCode,
                        CreatedAt = r.CreatedAt,
                        ClaimedAt = r.ClaimedAt,
                        IsExpired = isExpired,
                        DaysUntilExpiry = daysUntilExpiry
                    });
                }

                return new ApiResponse<List<RewardDto>>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RewardDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> MarkNotificationAsReadAsync(string notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                    return new ApiResponse<bool> { Success = false };

                notification.IsRead = true;
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

        public async Task<ApiResponse<bool>> ClaimRewardAsync(string customerId, string rewardId)
        {
            try
            {
                var reward = await _context.Rewards.FindAsync(rewardId);
                if (reward == null || reward.CustomerId != customerId)
                    return new ApiResponse<bool> { Success = false, Message = "المكافأة غير موجودة" };

                reward.Status = "claimed";
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Data = true, Message = "تم استلام المكافأة" };
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

        public async Task<ApiResponse<bool>> RateWashAsync(string washId, int rating, string comments)
        {
            try
            {
                var wash = await _context.WashHistories.FindAsync(washId);
                if (wash == null)
                    return new ApiResponse<bool> { Success = false };

                wash.Rating = rating;
                wash.CustomerComment = comments;
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

        public async Task<string> GetCustomerIdByUserIdAsync(string userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            return customer?.Id ?? string.Empty;
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequest request)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new ApiResponse<bool> { Success = false, Message = "العميل غير موجود" };

                if (!string.IsNullOrEmpty(request.Name))
                    customer.User.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Phone))
                    customer.User.Phone = request.Phone;

                customer.User.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "تم تحديث الملف الشخصي بنجاح",
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
    }
}
