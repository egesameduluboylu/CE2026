using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class AppPermission
{
    [MaxLength(200)]
    public string Key { get; set; } = default!; // "users.read"

    [MaxLength(256)]
    public string? Description { get; set; }
}
