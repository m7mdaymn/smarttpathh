using backend.DTOs;
using backend.Services;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using System.IO;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CustomerController(ICustomerService customerService, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _customerService = customerService;
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Debug endpoint to verify authorization is working
        /// </summary>
        [HttpGet("debug/test-auth")]
        public IActionResult TestAuth()
        {
            return Ok(new
            {
                success = true,
                message = "✅ Authorization successful! You have customer role.",
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(ClaimTypes.Email)?.Value,
                role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        [HttpGet("{customerId}/profile")]
        public async Task<IActionResult> GetProfile(string customerId)
        {
            var result = await _customerService.GetCustomerProfileAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{customerId}/loyalty-cards")]
        public async Task<IActionResult> GetLoyaltyCards(string customerId)
        {
            var result = await _customerService.GetLoyaltyCardsAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{customerId}/wash-history")]
        public async Task<IActionResult> GetWashHistory(string customerId)
        {
            var result = await _customerService.GetWashHistoryAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{customerId}/notifications")]
        public async Task<IActionResult> GetNotifications(string customerId)
        {
            var result = await _customerService.GetNotificationsAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{customerId}/rewards")]
        public async Task<IActionResult> GetRewards(string customerId)
        {
            var result = await _customerService.GetRewardsAsync(customerId);
            
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("notification/{notificationId}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            var result = await _customerService.MarkNotificationAsReadAsync(notificationId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{customerId}/reward/{rewardId}/claim")]
        public async Task<IActionResult> ClaimReward(string customerId, string rewardId)
        {
            var result = await _customerService.ClaimRewardAsync(customerId, rewardId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("wash/{washId}/rate")]
        public async Task<IActionResult> RateWash(string washId, [FromBody] RateWashRequest request)
        {
            var result = await _customerService.RateWashAsync(washId, request.Rating, request.Comments);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Additional routes to match frontend expectations
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfileByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<CustomerProfileDto> { Success = false, Message = "Customer not found" });
            
            return await GetProfile(customerId);
        }

        [HttpGet("washes/{userId}")]
        public async Task<IActionResult> GetWashesByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<List<WashHistoryDto>> { Success = false, Message = "Customer not found" });
            
            return await GetWashHistory(customerId);
        }

        [HttpGet("loyalty-cards/{userId}")]
        public async Task<IActionResult> GetLoyaltyCardsByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<List<LoyaltyCardDto>> { Success = false, Message = "Customer not found" });
            
            return await GetLoyaltyCards(customerId);
        }

        [HttpGet("notifications/{userId}")]
        public async Task<IActionResult> GetNotificationsByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<List<NotificationDto>> { Success = false, Message = "Customer not found" });
            
            return await GetNotifications(customerId);
        }

        [HttpGet("rewards/{userId}")]
        public async Task<IActionResult> GetRewardsByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<List<RewardDto>> { Success = false, Message = "Customer not found" });
            
            return await GetRewards(customerId);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerId = await _customerService.GetCustomerIdByUserIdAsync(request.UserId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Customer not found" });

            var result = await _customerService.UpdateProfileAsync(customerId, request);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("rewards/{rewardId}/claim")]
        public async Task<IActionResult> ClaimRewardByUserId(string rewardId, [FromBody] ClaimRewardRequest request)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(request.UserId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Customer not found" });

            return await ClaimReward(customerId, rewardId);
        }

        // ✅ Car Photos Endpoints
        [HttpPost("{customerId}/car-photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCarPhoto(string customerId, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "لم يتم اختيار صورة" });

            try
            {
                // Verify customer exists
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "العميل غير موجود" });

                // Check if customer already has a photo
                var existingPhoto = await _context.CarPhotos.FirstOrDefaultAsync(p => p.CustomerId == customerId);
                if (existingPhoto != null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "لديك صورة سيارة بالفعل" });

                // Save photo
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "car-photos");
                Directory.CreateDirectory(uploadsFolder);
                
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var carPhoto = new CarPhoto
                {
                    CustomerId = customerId,
                    PhotoUrl = $"/uploads/car-photos/{fileName}",
                    UploadedAt = DateTime.UtcNow
                };

                _context.CarPhotos.Add(carPhoto);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<CarPhotoDto>
                {
                    Success = true,
                    Message = "تم رفع الصورة بنجاح",
                    Data = new CarPhotoDto
                    {
                        Id = carPhoto.Id,
                        PhotoUrl = carPhoto.PhotoUrl,
                        UploadedAt = carPhoto.UploadedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{customerId}/car-photos")]
        public async Task<IActionResult> GetCarPhotos(string customerId)
        {
            var photos = await _context.CarPhotos
                .Where(p => p.CustomerId == customerId)
                .Select(p => new CarPhotoDto
                {
                    Id = p.Id,
                    PhotoUrl = p.PhotoUrl,
                    UploadedAt = p.UploadedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<CarPhotoDto>>
            {
                Success = true,
                Data = photos
            });
        }

        [HttpDelete("car-photo/{photoId}")]
        public async Task<IActionResult> DeleteCarPhoto(string photoId)
        {
            var photo = await _context.CarPhotos.FindAsync(photoId);
            if (photo == null)
                return NotFound(new ApiResponse<bool> { Success = false, Message = "الصورة غير موجودة" });

            _context.CarPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool> { Success = true, Message = "تم حذف الصورة", Data = true });
        }

        [HttpPost("car-photos/{userId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCarPhotoByUserId(string userId, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "لم يتم اختيار صورة" });

            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            return await UploadCarPhoto(customerId, photo);
        }

        [HttpGet("car-photos/{userId}")]
        public async Task<IActionResult> GetCarPhotosByUserId(string userId)
        {
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
                return NotFound(new ApiResponse<List<CarPhotoDto>> { Success = false, Message = "Customer not found" });

            return await GetCarPhotos(customerId);
        }
    }

    public class ClaimRewardRequest
    {
        public string UserId { get; set; }
    }

    public class RateWashRequest
    {
        public int Rating { get; set; }
        public string Comments { get; set; }
    }

    public class CarPhotoDto
    {
        public string Id { get; set; }
        public string PhotoUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
