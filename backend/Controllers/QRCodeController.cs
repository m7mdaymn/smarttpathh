using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(IQRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Generate a unique QR code for the current customer
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GenerateQRCode()
        {
            // Get customer ID from claims
            var customerIdClaim = User.FindFirst("customerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "معرّف العميل غير موجود في التوكن"
                });

            var result = await _qrCodeService.GenerateCustomerQRCodeAsync(customerIdClaim);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get the QR code for a customer
        /// If doesn't exist, generates a new one
        /// </summary>
        [HttpGet("my-code")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetMyQRCode()
        {
            var customerIdClaim = User.FindFirst("customerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "معرّف العميل غير موجود في التوكن"
                });

            var result = await _qrCodeService.GetCustomerQRCodeAsync(customerIdClaim);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get QR code for a specific customer (admin only)
        /// </summary>
        [HttpGet("{customerId}")]
        [Authorize(Roles = "merchant,superadmin")]
        public async Task<IActionResult> GetCustomerQRCode(string customerId)
        {
            var result = await _qrCodeService.GetCustomerQRCodeAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Validate a QR code
        /// Used by merchants before processing a wash
        /// </summary>
        [HttpPost("validate")]
        [Authorize(Roles = "merchant")]
        public async Task<IActionResult> ValidateQRCode([FromBody] ValidateQRCodeRequest request)
        {
            if (string.IsNullOrEmpty(request?.QRCode))
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "رمز QR مطلوب"
                });

            var result = await _qrCodeService.ValidateQRCodeAsync(request.QRCode);
            return Ok(result);
        }

        /// <summary>
        /// Get QR code as image (PNG)
        /// Accepts either customerId or userId - will resolve to customerId
        /// </summary>
        [HttpGet("{id}/image")]
        [Authorize(Roles = "customer,merchant,superadmin")]
        public async Task<IActionResult> GetQRCodeImage(string id, [FromQuery] int size = 200)
        {
            // Try to resolve as customerId first, then as userId
            var qrCodeResult = await _qrCodeService.GetCustomerQRCodeAsync(id);
            if (!qrCodeResult.Success)
            {
                // Try resolving from userId
                var customerIdResult = await _qrCodeService.GetCustomerIdByUserIdAsync(id);
                if (!customerIdResult.Success)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "العميل غير موجود" });
                
                qrCodeResult = await _qrCodeService.GetCustomerQRCodeAsync(customerIdResult.Data);
                if (!qrCodeResult.Success)
                    return NotFound(qrCodeResult);
            }

            var imageResult = await _qrCodeService.GenerateQRCodeImageAsync(qrCodeResult.Data, size);
            if (!imageResult.Success)
                return BadRequest(imageResult);

            return File(imageResult.Data, "image/png", $"qr-{id}.png");
        }

        // ========== Merchant Registration QR Code Endpoints ==========

        /// <summary>
        /// Get merchant registration QR code (for merchant dashboard)
        /// Returns QR code that customers can scan to register
        /// </summary>
        [HttpGet("merchant/registration")]
        [Authorize(Roles = "merchant")]
        public async Task<IActionResult> GetMerchantRegistrationQRCode()
        {
            var merchantIdClaim = User.FindFirst("merchantId")?.Value;
            if (string.IsNullOrEmpty(merchantIdClaim))
                return Unauthorized(new ApiResponse<MerchantQRCodeDto>
                {
                    Success = false,
                    Message = "معرّف المزود غير موجود في التوكن"
                });

            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _qrCodeService.GetMerchantRegistrationQRCodeAsync(merchantIdClaim, baseUrl);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Generate new merchant registration QR code
        /// </summary>
        [HttpPost("merchant/registration/generate")]
        [Authorize(Roles = "merchant")]
        public async Task<IActionResult> GenerateMerchantRegistrationQRCode()
        {
            var merchantIdClaim = User.FindFirst("merchantId")?.Value;
            if (string.IsNullOrEmpty(merchantIdClaim))
                return Unauthorized(new ApiResponse<MerchantQRCodeDto>
                {
                    Success = false,
                    Message = "معرّف المزود غير موجود في التوكن"
                });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _qrCodeService.GenerateMerchantRegistrationQRCodeAsync(merchantIdClaim, baseUrl);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get reward QR code as image (PNG)
        /// Used to generate QR codes for rewards that customers can show to merchants
        /// </summary>
        [HttpGet("reward/{rewardQRCode}/image")]
        [AllowAnonymous] // Allow without auth for simpler frontend display
        public async Task<IActionResult> GetRewardQRCodeImage(string rewardQRCode, [FromQuery] int size = 200)
        {
            if (string.IsNullOrEmpty(rewardQRCode))
                return BadRequest(new ApiResponse<string> { Success = false, Message = "رمز المكافأة مطلوب" });

            // Decode the QR code if it was URL encoded
            var decodedQRCode = System.Web.HttpUtility.UrlDecode(rewardQRCode);
            
            // Generate QR code image for the reward code - use GenerateAnyQRCodeImageAsync (no customer validation)
            var imageResult = await _qrCodeService.GenerateAnyQRCodeImageAsync(decodedQRCode, size);
            if (!imageResult.Success)
                return BadRequest(imageResult);

            return File(imageResult.Data, "image/png", $"reward-qr.png");
        }

        /// <summary>
        /// Get merchant info by ID (for customer registration page after QR scan)
        /// PUBLIC endpoint - no auth required
        /// </summary>
        [HttpGet("merchant/{merchantId}/info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMerchantInfo(string merchantId)
        {
            var result = await _qrCodeService.GetMerchantInfoAsync(merchantId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Validate merchant registration code (for manual customer registration)
        /// PUBLIC endpoint - no auth required
        /// </summary>
        [HttpPost("merchant/validate-code")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateMerchantCode([FromBody] ValidateMerchantCodeRequest request)
        {
            if (string.IsNullOrEmpty(request?.Code))
                return BadRequest(new ApiResponse<MerchantInfoDto>
                {
                    Success = false,
                    Message = "رمز التسجيل مطلوب"
                });

            var result = await _qrCodeService.ValidateMerchantCodeAsync(request.Code);
            return Ok(result);
        }
    }

    /// <summary>
    /// Request model for QR code validation
    /// </summary>
    public class ValidateQRCodeRequest
    {
        public string QRCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for merchant code validation
    /// </summary>
    public class ValidateMerchantCodeRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}

