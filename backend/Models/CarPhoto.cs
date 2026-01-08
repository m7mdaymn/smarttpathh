using System;

namespace backend.Models
{
    public class CarPhoto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string CustomerId { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
        
        // MerchantId is optional - only set when merchant uploads photo
        public string? MerchantId { get; set; }
        public Merchant? Merchant { get; set; }
        
        public string PhotoUrl { get; set; } = string.Empty; // Path relative to wwwroot, or full URL
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
