using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class AppRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public string Name { get; set; } = default!; // "admin"

    [MaxLength(256)]
    public string? Description { get; set; }
}