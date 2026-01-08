using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public AuthService(ApplicationDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"🔐 [AUTH] Login attempt for email: {request.Email}");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    Console.WriteLine($"❌ [AUTH] User not found for email: {request.Email}");
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة",
                        Errors = new List<string> { "المستخدم غير موجود" }
                    };
                }

                // ✅ **التحقق من كلمة المرور باستخدام BCrypt**
                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash);
                }
                catch
                {
                    // Fallback to regular verify if enhanced fails
                    try { isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash); }
                    catch { isPasswordValid = false; }
                }
                
                if (!isPasswordValid)
                {
                    Console.WriteLine($"❌ [AUTH] Password verification failed for user: {user.Id}");
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة",
                        Errors = new List<string> { "كلمة المرور غير صحيحة" }
                    };
                }

                if (!user.IsActive)
                {
                    Console.WriteLine($"❌ [AUTH] User account disabled for user: {user.Id}");
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "تم تعطيل حسابك",
                        Errors = new List<string> { "الحساب غير نشط" }
                    };
                }

                Console.WriteLine($"✅ [AUTH] User verified: {user.Id}, Role: {user.Role}");

                // Get merchant subscription status if user is a merchant
                string? subscriptionStatus = null;
                string? businessName = null;
                string? merchantId = null;
                string? merchantName = null;
                bool? hasMerchant = null;
                string? customerId = null;

                if (user.Role == "merchant")
                {
                    var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.UserId == user.Id);
                    if (merchant != null)
                    {
                        subscriptionStatus = merchant.SubscriptionStatus;
                        businessName = merchant.BusinessName;
                        merchantId = merchant.Id;
                        Console.WriteLine($"📋 [AUTH] Merchant subscription status: {subscriptionStatus}");
                    }
                }
                else if (user.Role == "customer")
                {
                    // Get customer's linked merchant info
                    var customer = await _context.Customers
                        .Include(c => c.Merchant)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);
                    if (customer != null)
                    {
                        hasMerchant = !string.IsNullOrEmpty(customer.MerchantId);
                        merchantId = customer.MerchantId;
                        merchantName = customer.Merchant?.BusinessName;
                        customerId = customer.Id;
                        Console.WriteLine($"📋 [AUTH] Customer linked to merchant: {merchantName}");
                    }
                }

                // Generate JWT Token with role-specific IDs
                var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.Role, 
                    user.Role == "merchant" ? merchantId : null, 
                    user.Role == "customer" ? customerId : null);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    SubscriptionStatus = subscriptionStatus,
                    BusinessName = businessName,
                    MerchantId = merchantId,
                    MerchantName = merchantName,
                    HasMerchant = hasMerchant
                };

                return new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "تم تسجيل الدخول بنجاح",
                    Data = new LoginResponse
                    {
                        Token = token,
                        User = userDto
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AUTH] Login exception: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "حدث خطأ أثناء تسجيل الدخول",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterCustomerAsync(RegisterCustomerRequest request)
        {
            try
            {
                // Validate merchant ID or code is provided
                string? merchantId = request.MerchantId;
                Merchant? merchant = null;

                // If MerchantId provided directly (from QR scan)
                if (!string.IsNullOrEmpty(merchantId))
                {
                    merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId);
                }
                // If MerchantCode provided (manual entry)
                else if (!string.IsNullOrEmpty(request.MerchantCode))
                {
                    merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.RegistrationCode == request.MerchantCode);
                    if (merchant != null)
                    {
                        merchantId = merchant.Id;
                    }
                }

                // Merchant is required
                if (merchant == null || string.IsNullOrEmpty(merchantId))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "يجب ربط حسابك بمغسلة. يرجى مسح رمز QR الخاص بالمغسلة أو إدخال رمز التسجيل",
                        Errors = new List<string> { "المغسلة غير موجودة أو لم يتم تحديدها" }
                    };
                }

                // Check if merchant subscription is active
                if (merchant.SubscriptionStatus != "active")
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "هذه المغسلة غير مفعلة حالياً. يرجى التواصل مع المغسلة",
                        Errors = new List<string> { "المغسلة غير نشطة" }
                    };
                }

                // التحقق من عدم وجود المستخدم
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "هذا البريد الإلكتروني مسجل بالفعل",
                        Errors = new List<string> { "البريد موجود" }
                    };

                var userId = Guid.NewGuid().ToString();
                var customerId = Guid.NewGuid().ToString();

                // ✅ **تجزئة كلمة المرور باستخدام BCrypt**
                var user = new User
                {
                    Id = userId,
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 13),
                    Role = "customer"
                };

                var customer = new Customer
                {
                    Id = customerId,
                    UserId = userId,
                    MerchantId = merchantId, // Link to merchant
                    PlateNumber = request.GetPlateNumber(), // Use helper method for compatibility
                    QRCode = $"DP-CUST-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    QRCodeGeneratedAt = DateTime.UtcNow
                };

                user.Customer = customer;

                // Get merchant settings for loyalty card creation
                var merchantSettings = await _context.MerchantSettings
                    .FirstOrDefaultAsync(s => s.MerchantId == merchantId);

                // Create loyalty card for the customer with merchant settings
                var loyaltyCardId = Guid.NewGuid().ToString();
                var loyaltyCard = new LoyaltyCard
                {
                    Id = loyaltyCardId,
                    CustomerId = customerId,
                    MerchantId = merchantId,
                    WashesCompleted = 0,
                    WashesRequired = merchantSettings?.RewardWashesRequired ?? 5,
                    IsActive = true,
                    IsPaused = false,
                    IsRewardClaimed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(merchantSettings?.RewardTimeLimitDays ?? 30)
                };

                _context.LoyaltyCards.Add(loyaltyCard);

                // Update merchant's customer count
                merchant.TotalCustomers++;
                merchant.UpdatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "تم إنشاء الحساب بنجاح وربطه بـ " + merchant.BusinessName,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        MerchantId = merchantId,
                        MerchantName = merchant.BusinessName,
                        HasMerchant = true
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "حدث خطأ أثناء التسجيل",
                    Errors = new List<string> { ex.Message, ex.InnerException?.Message ?? string.Empty }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterMerchantAsync(RegisterMerchantRequest request)
        {
            try
            {
                // التحقق من عدم وجود المستخدم
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "هذا البريد الإلكتروني مسجل بالفعل",
                        Errors = new List<string> { "البريد موجود" }
                    };

                // إنشاء IDs
                var userId = Guid.NewGuid().ToString();
                var merchantId = Guid.NewGuid().ToString();
                var settingsId = Guid.NewGuid().ToString();

                // ✅ **تجزئة كلمة المرور باستخدام BCrypt**
                var user = new User
                {
                    Id = userId,
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 13),
                    Role = "merchant"
                };

                // إنشاء Merchant
                var merchant = new Merchant
                {
                    Id = merchantId,
                    UserId = userId,
                    BusinessName = request.BusinessName,
                    BusinessType = request.BusinessType,
                    City = request.City,
                    BranchName = request.BranchName,
                    Plan = request.SubscriptionType,
                    PlanExpiryDate = DateTime.UtcNow.AddDays(30),
                    SubscriptionStatus = "pending",
                    RegistrationCode = Merchant.GenerateRegistrationCode(),
                    QRCodeImageUrl = null
                };

                var settings = new MerchantSettings
                {
                    Id = settingsId,
                    MerchantId = merchantId,
                    RewardWashesRequired = 5,
                    RewardTimeLimitDays = 30,
                    AntiFraudSameDay = true,
                    EnableCarPhoto = false,
                    NotificationsEnabled = true,
                    IsLoyaltyPaused = false,
                    LoyaltyPausedUntil = null,

                    // Notification templates
                    NotificationTemplateWelcome = "مرحباً بك في {BusinessName}!",
                    NotificationTemplateRemaining = "باقي {Remaining} غسلات للحصول على مكافأتك!",
                    NotificationTemplateRewardClose = "أنت قريب جداً! غسلة واحدة فقط للحصول على مكافأتك!",
                    CustomPrimaryColor = "#3B82F6",
                    CustomSecondaryColor = "#0F172A",
                    CustomBusinessTagline = "",
                    RewardDescription = "Free wash on completion",
                    RewardDescriptionAr = "غسلة مجانية عند الإكمال"
                };

                // ربط العلاقات
                user.Merchant = merchant;
                merchant.Settings = new List<MerchantSettings> { settings };

                // الحفظ
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "تم إنشاء حساب التاجر بنجاح",
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = request.Phone,
                        Role = user.Role,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "حدث خطأ أثناء التسجيل",
                    Errors = new List<string> {
                        ex.Message,
                        ex.InnerException?.Message ?? string.Empty
                    }
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await Task.FromResult(true);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email) ?? new User();
        }
    }
}