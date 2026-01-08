using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "merchant")]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetMerchantIdByUserId(string userId)
        {
            var result = await _merchantService.GetMerchantIdByUserIdAsync(userId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{merchantId}/profile")]
        public async Task<IActionResult> GetProfile(string merchantId)
        {
            var result = await _merchantService.GetMerchantProfileAsync(merchantId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{merchantId}/dashboard")]
        public async Task<IActionResult> GetDashboard(string merchantId)
        {
            var result = await _merchantService.GetDashboardAsync(merchantId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{merchantId}/customers")]
        public async Task<IActionResult> GetCustomers(string merchantId)
        {
            var result = await _merchantService.GetCustomersAsync(merchantId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{merchantId}/settings")]
        public async Task<IActionResult> GetSettings(string merchantId)
        {
            var result = await _merchantService.GetSettingsAsync(merchantId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/settings")]
        public async Task<IActionResult> UpdateSettings(string merchantId, [FromBody] MerchantSettingsDto settings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _merchantService.UpdateSettingsAsync(merchantId, settings);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{merchantId}/scan-qr")]
        public async Task<IActionResult> ProcessQRScan([FromBody] ProcessWashRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _merchantService.ProcessQRScanAsync(request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{merchantId}/validate-customer-qr")]
        public async Task<IActionResult> ValidateCustomerQR(string merchantId, [FromBody] ValidateCustomerQRRequest request)
        {
            if (string.IsNullOrEmpty(request?.CustomerQRCode))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "رمز QR مطلوب" });

            var result = await _merchantService.GetCustomerByQRCodeAsync(merchantId, request.CustomerQRCode);
            return Ok(result);
        }

        [HttpPost("{merchantId}/record-wash")]
        public async Task<IActionResult> RecordWash(string merchantId, [FromBody] ProcessWashRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure merchantId from route matches request
            request.MerchantId = merchantId;

            var result = await _merchantService.RecordWashAsync(request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/customers/{customerId}")]
        public async Task<IActionResult> UpdateCustomer(string merchantId, string customerId, [FromBody] MerchantCustomerUpdateDto customerData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _merchantService.UpdateCustomerAsync(merchantId, customerId, customerData);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{merchantId}/customers/{customerId}")]
        public async Task<IActionResult> DeleteCustomer(string merchantId, string customerId)
        {
            var result = await _merchantService.DeleteCustomerAsync(merchantId, customerId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/customers/{customerId}/activate")]
        public async Task<IActionResult> ActivateCustomer(string merchantId, string customerId)
        {
            var result = await _merchantService.ToggleCustomerStatusAsync(merchantId, customerId, true);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/customers/{customerId}/deactivate")]
        public async Task<IActionResult> DeactivateCustomer(string merchantId, string customerId)
        {
            var result = await _merchantService.ToggleCustomerStatusAsync(merchantId, customerId, false);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{merchantId}/loyalty-card")]
        public async Task<IActionResult> CreateLoyaltyCard(string merchantId, [FromBody] CreateLoyaltyCardRequest request)
        {
            var result = await _merchantService.CreateLoyaltyCardAsync(merchantId, request.CustomerId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/password")]
        public async Task<IActionResult> UpdatePassword(string merchantId, [FromBody] UpdatePasswordRequest request)
        {
            var result = await _merchantService.UpdatePasswordAsync(merchantId, request.CurrentPassword, request.NewPassword);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{merchantId}/profile")]
        public async Task<IActionResult> UpdateProfile(string merchantId, [FromBody] MerchantProfileDto profile)
        {
            var result = await _merchantService.UpdateProfileAsync(merchantId, profile);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{merchantId}/logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(string merchantId, IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "لم يتم اختيار صورة" });

            try
            {
                // Save logo to wwwroot/uploads/logos
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                Directory.CreateDirectory(uploadsFolder);
                
                var fileName = $"{merchantId}_{Guid.NewGuid()}{Path.GetExtension(logo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                var logoUrl = $"/uploads/logos/{fileName}";
                var result = await _merchantService.UploadLogoAsync(merchantId, logoUrl);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{merchantId}/validate-reward")]
        public async Task<IActionResult> ValidateRewardQR(string merchantId, [FromBody] ValidateRewardRequest request)
        {
            if (string.IsNullOrEmpty(request?.RewardQRCode))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "رمز المكافأة مطلوب" });

            var result = await _merchantService.ValidateRewardQRCodeAsync(merchantId, request.RewardQRCode);
            return Ok(result);
        }

        [HttpPost("{merchantId}/redeem-reward")]
        public async Task<IActionResult> RedeemReward(string merchantId, [FromBody] RedeemRewardRequest request)
        {
            if (string.IsNullOrEmpty(request?.RewardQRCode))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "رمز المكافأة مطلوب" });

            var result = await _merchantService.RedeemRewardAsync(merchantId, request.RewardQRCode);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

    public class CreateLoyaltyCardRequest
    {
        public string CustomerId { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ValidateRewardRequest
    {
        public string RewardQRCode { get; set; }
    }

    public class RedeemRewardRequest
    {
        public string RewardQRCode { get; set; }
    }
}
