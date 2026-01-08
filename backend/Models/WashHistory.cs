using System;

namespace backend.Models
{
    public class WashHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
        
        public string MerchantId { get; set; } = string.Empty;
        public Merchant? Merchant { get; set; }
        
        public DateTime WashDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "completed"; // completed, pending
        
        // Merchant fills this when scanning QR
        public string? ServiceDescription { get; set; } // What was done in this wash
        public decimal Price { get; set; } = 0; // Price charged for this wash
        
        // Customer feedback (added later by customer)
        public int? Rating { get; set; } // 1-5 stars
        public string? CustomerComment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
