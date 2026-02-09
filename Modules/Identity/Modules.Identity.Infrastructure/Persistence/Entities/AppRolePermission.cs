using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class AppRolePermission
{
    [Required]
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string PermissionKey { get; set; } = default!;
    public AppPermission Permission { get; set; } = default!;
}
