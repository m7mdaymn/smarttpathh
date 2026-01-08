using System;

namespace backend.Models
{
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // success, warning, info, promotion, error
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
