using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace backend.Services.Implementations
{
    /// <summary>
    /// QR Code generation and validation service
    /// Generates unique QR codes for customers and merchants
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public QRCodeService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ========== Customer QR Code Methods ==========

        /// <summary>
        /// Generate a unique QR code for a customer (format: DP-CUST-{ID}-{HASH})
        /// </summary>
        public async Task<ApiResponse<string>> GenerateCustomerQRCodeAsync(string customerId)
        {
            try
            {
                // Check if customer exists
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
                if (customer == null)
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "العميل غير موجود"
                    };

                // Generate unique QR code: DP-CUST-{ID}-{TIMESTAMP-HASH}
                string qrCode = GenerateUniqueCustomerQRCode(customerId);

                // Save to customer
                customer.QRCode = qrCode;
                customer.QRCodeGeneratedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "تم إنشاء رمز QR بنجاح",
                    Data = qrCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "خطأ في إنشاء رمز QR",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get the existing QR code for a customer
        /// If doesn't exist, generate a new one
        /// </summary>
        public async Task<ApiResponse<string>> GetCustomerQRCodeAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
                if (customer == null)
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "العميل غير موجود"
                    };

                // If customer doesn't have a QR code, generate one
                if (string.IsNullOrEmpty(customer.QRCode))
                {
                    return await GenerateCustomerQRCodeAsync(customerId);
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "تم جلب رمز QR بنجاح",
                    Data = customer.QRCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "خطأ في جلب رمز QR",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Validate a QR code - check if it exists and belongs to an active customer
        /// </summary>
        public async Task<ApiResponse<bool>> ValidateQRCodeAsync(string qrCode)
        {
            try
            {
                if (string.IsNullOrEmpty(qrCode))
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "رمز QR غير صحيح",
                        Data = false
                    };

                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.QRCode == qrCode);

                if (customer == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "رمز QR غير موجود",
                        Data = false
                    };

                // Check if customer account is active
                if (!customer.User.IsActive)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "حساب العميل غير مفعّل",
                        Data = false
                    };

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "رمز QR صحيح",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "خطأ في التحقق من رمز QR",
                    Errors = new List<string> { ex.Message },
                    Data = false
                };
            }
        }

        /// <summary>
        /// Generate QR code image as PNG using QRCoder library (validates customer QR codes)
        /// </summary>
        public async Task<ApiResponse<byte[]>> GenerateQRCodeImageAsync(string qrCode, int size = 200)
        {
            try
            {
                // Validate QR code exists (only for customer codes)
                var isValid = await ValidateQRCodeAsync(qrCode);
                if (!isValid.Data)
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "رمز QR غير صحيح"
                    };

                // Generate QR code image using QRCoder
                byte[] qrCodeImage = GenerateQRCodeImage(qrCode, size);

                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "تم إنشاء صورة QR بنجاح",
                    Data = qrCodeImage
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "خطأ في إنشاء صورة QR",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Generate QR code image for any code (reward codes, etc.) WITHOUT validation
        /// Used for reward QR codes where we just need to create the image
        /// </summary>
        public Task<ApiResponse<byte[]>> GenerateAnyQRCodeImageAsync(string code, int size = 200)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Task.FromResult(new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "رمز QR مطلوب"
                    });
                }

                // Generate QR code image using QRCoder
                byte[] qrCodeImage = GenerateQRCodeImage(code, size);

                return Task.FromResult(new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "تم إنشاء صورة QR بنجاح",
                    Data = qrCodeImage
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "خطأ في إنشاء صورة QR",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ========== Merchant Registration QR Code Methods ==========

        /// <summary>
        /// Generate a QR code for merchant customer registration
        /// </summary>
        public async Task<ApiResponse<MerchantQRCodeDto>> GenerateMerchantRegistrationQRCodeAsync(string merchantId, string baseUrl)
        {
            try
            {
                var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId);
                if (merchant == null)
                    return new ApiResponse<MerchantQRCodeDto>
                    {
                        Success = false,
                        Message = "المزود غير موجود"
                    };

                // Generate registration code if not exists
                if (string.IsNullOrEmpty(merchant.RegistrationCode))
                {
                    merchant.RegistrationCode = Merchant.GenerateRegistrationCode();
                }

                // Use frontend URL from configuration instead of backend base URL
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
                string registrationUrl = $"{frontendUrl.TrimEnd('/')}/auth/register/{merchantId}";

                // Generate QR code image as Base64
                byte[] qrCodeBytes = GenerateQRCodeImage(registrationUrl, 300);
                string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
                merchant.QRCodeImageUrl = $"data:image/png;base64,{qrCodeBase64}";
                merchant.QRCodeGeneratedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<MerchantQRCodeDto>
                {
                    Success = true,
                    Message = "تم إنشاء رمز QR للتسجيل بنجاح",
                    Data = new MerchantQRCodeDto
                    {
                        MerchantId = merchant.Id,
                        BusinessName = merchant.BusinessName,
                        RegistrationCode = merchant.RegistrationCode,
                        QRCodeBase64 = merchant.QRCodeImageUrl,
                        RegistrationUrl = registrationUrl,
                        GeneratedAt = merchant.QRCodeGeneratedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantQRCodeDto>
                {
                    Success = false,
                    Message = "خطأ في إنشاء رمز QR",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get existing merchant registration QR code or generate new one
        /// </summary>
        public async Task<ApiResponse<MerchantQRCodeDto>> GetMerchantRegistrationQRCodeAsync(string merchantId, string baseUrl)
        {
            try
            {
                var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId);
                if (merchant == null)
                    return new ApiResponse<MerchantQRCodeDto>
                    {
                        Success = false,
                        Message = "المزود غير موجود"
                    };

                // If QR code doesn't exist, generate one
                if (string.IsNullOrEmpty(merchant.QRCodeImageUrl) || string.IsNullOrEmpty(merchant.RegistrationCode))
                {
                    return await GenerateMerchantRegistrationQRCodeAsync(merchantId, baseUrl);
                }

                // Use frontend URL from configuration
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
                string registrationUrl = $"{frontendUrl.TrimEnd('/')}/auth/register/{merchantId}";

                return new ApiResponse<MerchantQRCodeDto>
                {
                    Success = true,
                    Message = "تم جلب رمز QR بنجاح",
                    Data = new MerchantQRCodeDto
                    {
                        MerchantId = merchant.Id,
                        BusinessName = merchant.BusinessName,
                        RegistrationCode = merchant.RegistrationCode,
                        QRCodeBase64 = merchant.QRCodeImageUrl,
                        RegistrationUrl = registrationUrl,
                        GeneratedAt = merchant.QRCodeGeneratedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantQRCodeDto>
                {
                    Success = false,
                    Message = "خطأ في جلب رمز QR",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Validate merchant registration code (6-digit code for manual entry)
        /// </summary>
        public async Task<ApiResponse<MerchantInfoDto>> ValidateMerchantCodeAsync(string registrationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(registrationCode) || registrationCode.Length != 6)
                    return new ApiResponse<MerchantInfoDto>
                    {
                        Success = false,
                        Message = "رمز التسجيل غير صحيح"
                    };

                var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.RegistrationCode == registrationCode);
                if (merchant == null)
                    return new ApiResponse<MerchantInfoDto>
                    {
                        Success = false,
                        Message = "رمز التسجيل غير موجود"
                    };

                // Check if merchant has active subscription
                if (merchant.SubscriptionStatus != "active")
                    return new ApiResponse<MerchantInfoDto>
                    {
                        Success = false,
                        Message = "اشتراك المزود غير نشط"
                    };

                return new ApiResponse<MerchantInfoDto>
                {
                    Success = true,
                    Message = "رمز التسجيل صحيح",
                    Data = new MerchantInfoDto
                    {
                        Id = merchant.Id,
                        BusinessName = merchant.BusinessName,
                        Plan = merchant.Plan,
                        IsActive = merchant.SubscriptionStatus == "active"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantInfoDto>
                {
                    Success = false,
                    Message = "خطأ في التحقق من رمز التسجيل",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get merchant info by ID (for registration page after QR scan)
        /// </summary>
        public async Task<ApiResponse<MerchantInfoDto>> GetMerchantInfoAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId);
                if (merchant == null)
                    return new ApiResponse<MerchantInfoDto>
                    {
                        Success = false,
                        Message = "المزود غير موجود"
                    };

                // Check if merchant has active subscription
                if (merchant.SubscriptionStatus != "active")
                    return new ApiResponse<MerchantInfoDto>
                    {
                        Success = false,
                        Message = "اشتراك المزود غير نشط"
                    };

                return new ApiResponse<MerchantInfoDto>
                {
                    Success = true,
                    Message = "تم جلب معلومات المزود بنجاح",
                    Data = new MerchantInfoDto
                    {
                        Id = merchant.Id,
                        BusinessName = merchant.BusinessName,
                        Plan = merchant.Plan,
                        IsActive = merchant.SubscriptionStatus == "active"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MerchantInfoDto>
                {
                    Success = false,
                    Message = "خطأ في جلب معلومات المزود",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ========== Private Helper Methods ==========

        /// <summary>
        /// Generate a unique QR code in format: DP-CUST-{ID}-{HASH}
        /// </summary>
        private string GenerateUniqueCustomerQRCode(string customerId)
        {
            // Take first 8 chars of customer ID
            string idPart = customerId.Length > 8 ? customerId.Substring(0, 8) : customerId;

            // Generate hash from timestamp and ID
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string combined = $"{customerId}{timestamp}";
            
            // Simple hash: take last 6 chars of base64 encoded hash
            byte[] data = Encoding.UTF8.GetBytes(combined);
            string hash = Convert.ToBase64String(data)
                .Replace("=", "")
                .Replace("/", "")
                .Replace("+", "")
                .ToUpper();

            // Take first 6 chars for compact format
            if (hash.Length > 6)
                hash = hash.Substring(0, 6);

            return $"DP-CUST-{idPart.ToUpper()}-{hash}";
        }

        /// <summary>
        /// Generate QR code image as PNG bytes using QRCoder library
        /// </summary>
        private byte[] GenerateQRCodeImage(string content, int size = 200)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(size / 20); // pixels per module
        }

        /// <summary>
        /// Get customer ID by user ID
        /// </summary>
        public async Task<ApiResponse<string>> GetCustomerIdByUserIdAsync(string userId)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "العميل غير موجود"
                    };

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = customer.Id
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "خطأ في البحث عن العميل",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
