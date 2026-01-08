using backend.DTOs;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service for QR code generation and validation
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Generate a unique QR code for a customer
        /// </summary>
        Task<ApiResponse<string>> GenerateCustomerQRCodeAsync(string customerId);

        /// <summary>
        /// Get the QR code for a customer
        /// </summary>
        Task<ApiResponse<string>> GetCustomerQRCodeAsync(string customerId);

        /// <summary>
        /// Validate a QR code (check if it exists and is valid)
        /// </summary>
        Task<ApiResponse<bool>> ValidateQRCodeAsync(string qrCode);

        /// <summary>
        /// Generate QR code as image (PNG/SVG) - validates customer codes
        /// </summary>
        Task<ApiResponse<byte[]>> GenerateQRCodeImageAsync(string qrCode, int size = 200);

        /// <summary>
        /// Generate QR code image for any code WITHOUT validation (used for reward codes)
        /// </summary>
        Task<ApiResponse<byte[]>> GenerateAnyQRCodeImageAsync(string code, int size = 200);

        // ========== Merchant Registration QR Code Methods ==========

        /// <summary>
        /// Generate a QR code for merchant customer registration
        /// The QR code will link to the customer registration page with merchant ID
        /// </summary>
        Task<ApiResponse<MerchantQRCodeDto>> GenerateMerchantRegistrationQRCodeAsync(string merchantId, string baseUrl);

        /// <summary>
        /// Get existing merchant registration QR code or generate new one
        /// </summary>
        Task<ApiResponse<MerchantQRCodeDto>> GetMerchantRegistrationQRCodeAsync(string merchantId, string baseUrl);

        /// <summary>
        /// Validate merchant registration code (6-digit code for manual entry)
        /// </summary>
        Task<ApiResponse<MerchantInfoDto>> ValidateMerchantCodeAsync(string registrationCode);

        /// <summary>
        /// Get merchant info by ID (for registration page after QR scan)
        /// </summary>
        Task<ApiResponse<MerchantInfoDto>> GetMerchantInfoAsync(string merchantId);
        
        /// <summary>
        /// Get customer ID by user ID
        /// </summary>
        Task<ApiResponse<string>> GetCustomerIdByUserIdAsync(string userId);
    }

    /// <summary>
    /// DTO for merchant QR code response
    /// </summary>
    public class MerchantQRCodeDto
    {
        public string MerchantId { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string RegistrationCode { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string RegistrationUrl { get; set; } = string.Empty;
        public DateTime? GeneratedAt { get; set; }
    }
}
