using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class AppRole : BaseEntity
{
    [MaxLength(64)]
    public string Name { get; set; } = default!; // "admin"

    [MaxLength(256)]
    public string? Description { get; set; }
}