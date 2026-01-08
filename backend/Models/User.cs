using System;
using System.Collections.Generic;

namespace backend.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // customer, merchant, superadmin
        public bool IsActive { get; set; } = true; // Used for blocking user - cannot login when false
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Customer? Customer { get; set; }
        public Merchant? Merchant { get; set; }
    }
}
