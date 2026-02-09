using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class AppUserRole
{
    [Required]
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    [Required]
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = default!;
}
