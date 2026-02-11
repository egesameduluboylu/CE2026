using System;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public class UserLoginHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = "";
        public AppUser User { get; set; } = null!;
        
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public string IpAddress { get; set; } = "";
        public string? UserAgent { get; set; }
        public string? Location { get; set; } // City, Country
        public string? Device { get; set; } // Device type/browser
        
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
