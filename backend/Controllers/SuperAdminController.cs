using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "superadmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;

        public SuperAdminController(ISuperAdminService superAdminService)
        {
            _superAdminService = superAdminService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _superAdminService.GetDashboardAsync();
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _superAdminService.GetAllCustomersAsync();
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("merchants")]
        public async Task<IActionResult> GetAllMerchants()
        {
            var result = await _superAdminService.GetAllMerchantsAsync();
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("merchants/{merchantId}")]
        public async Task<IActionResult> UpdateMerchant(string merchantId, [FromBody] UpdateMerchantRequest request)
        {
            var result = await _superAdminService.UpdateMerchantAsync(merchantId, request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _superAdminService.GetStatisticsAsync();
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _superAdminService.GetSettingsAsync();
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] PlatformSettingsDto settings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _superAdminService.UpdateSettingsAsync(settings);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("customer/{customerId}/suspend")]
        public async Task<IActionResult> SuspendCustomer(string customerId)
        {
            var result = await _superAdminService.SuspendCustomerAsync(customerId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Suspend subscription - can login but cannot scan washes
        [HttpPut("merchant/{merchantId}/suspend")]
        public async Task<IActionResult> SuspendMerchant(string merchantId)
        {
            var result = await _superAdminService.SuspendSubscriptionAsync(merchantId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Block merchant - cannot login at all
        [HttpPut("merchant/{merchantId}/block")]
        public async Task<IActionResult> BlockMerchant(string merchantId)
        {
            var result = await _superAdminService.BlockMerchantAsync(merchantId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Unblock merchant - can login again
        [HttpPut("merchant/{merchantId}/unblock")]
        public async Task<IActionResult> UnblockMerchant(string merchantId)
        {
            var result = await _superAdminService.UnblockMerchantAsync(merchantId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("customer/{customerId}/activate")]
        public async Task<IActionResult> ActivateCustomer(string customerId)
        {
            var result = await _superAdminService.ActivateCustomerAsync(customerId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("merchant/{merchantId}/activate")]
        public async Task<IActionResult> ActivateMerchant(string merchantId, [FromBody] ActivateMerchantRequest? request)
        {
            var months = request?.Months ?? 1;
            var result = await _superAdminService.ActivateMerchantAsync(merchantId, months);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("merchant/{merchantId}/plan")]
        public async Task<IActionResult> UpdateMerchantPlan(string merchantId, [FromBody] UpdatePlanRequest request)
        {
            var result = await _superAdminService.UpdateMerchantPlanAsync(merchantId, request.NewPlan);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

    public class UpdatePlanRequest
    {
        public string NewPlan { get; set; } = "Basic";
    }

    public class ActivateMerchantRequest
    {
        public int Months { get; set; } = 1;
    }

    public class UpdateMerchantRequest
    {
        public string? BusinessName { get; set; }
        public string? OwnerName { get; set; }
        public string? City { get; set; }
        public string? Plan { get; set; }
    }
}
