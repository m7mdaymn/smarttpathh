using backend.DTOs;
using backend.Models;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<UserDto>> RegisterCustomerAsync(RegisterCustomerRequest request);
        Task<ApiResponse<UserDto>> RegisterMerchantAsync(RegisterMerchantRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<User> GetUserByEmailAsync(string email);
    }
}
