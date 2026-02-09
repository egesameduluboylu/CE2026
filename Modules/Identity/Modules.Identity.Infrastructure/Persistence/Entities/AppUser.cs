using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{

    public class AppUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int FailedLoginCount { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }
        public DateTimeOffset? LastFailedLoginAt { get; set; }
        public bool IsAdmin { get; set; }

    }
}
