using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public class AppPermission : BaseEntity
    {
        [MaxLength(200)]
        public string Key { get; set; } = default!; // "users.read"

        [MaxLength(256)]
        public string? Description { get; set; }
    }
}
