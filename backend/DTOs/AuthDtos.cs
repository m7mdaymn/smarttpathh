    namespace backend.DTOs
    {
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public UserDto User { get; set; } = new UserDto();
        }

        // Customer registration with merchant linking
        public class RegisterCustomerRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? PlateNumber { get; set; }
            public string? CarPlateNumber { get; set; } // Alias for frontend compatibility
            public string? CarPhoto { get; set; } // Only if merchant allows car photos
            public string? MerchantId { get; set; } // Merchant ID (from QR scan)
            public string? MerchantCode { get; set; } // Merchant registration code (manual entry)
            
            public string? GetPlateNumber() => PlateNumber ?? CarPlateNumber;
        }

        public class ValidateMerchantCodeRequest
        {
            public string Code { get; set; } = string.Empty;
        }

        public class MerchantInfoDto
        {
            public string Id { get; set; } = string.Empty;
            public string BusinessName { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string? Plan { get; set; }
            public bool IsActive { get; set; } = true;
            public bool EnableCarPhoto { get; set; } = false;
        }

        public class RegisterMerchantRequest
        {
            public string Name { get; set; } = string.Empty;
            public string BusinessName { get; set; } = string.Empty;
            public string BusinessType { get; set; } = "car_wash";
            public string City { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string SubscriptionType { get; set; } = "Basic"; // Basic, Pro
            public string PaymentMethod { get; set; } = string.Empty;
        }

        public class UserDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true; // User blocked status
            public string? SubscriptionStatus { get; set; } // For merchants: active, pending, expired, suspended
            public string? BusinessName { get; set; }
            public string? MerchantId { get; set; }
            public string? MerchantName { get; set; }
            public bool? HasMerchant { get; set; }
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
