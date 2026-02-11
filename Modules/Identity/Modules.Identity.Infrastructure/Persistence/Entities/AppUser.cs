using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public class AppUser : BaseEntity
    {
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        
        // Profile fields
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Status & tracking
        public DateTimeOffset? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public string? LastLoginUserAgent { get; set; }
        
        // Security fields
        public int FailedLoginCount { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }
        public DateTimeOffset? LastFailedLoginAt { get; set; }
        public bool IsAdmin { get; set; }
        
        // 2FA
        public string? UserTfaTokenId { get; set; }
        
        // Computed properties
        public string? FullName => string.IsNullOrWhiteSpace(FirstName) ? null : $"{FirstName} {LastName}".Trim();

        // Navigation properties
        public UserTfaToken? UserTfaToken { get; set; }
        public ICollection<UserBackupCode> UserBackupCodes { get; set; } = new List<UserBackupCode>();
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}
