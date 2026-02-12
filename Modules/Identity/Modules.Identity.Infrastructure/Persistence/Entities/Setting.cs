using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class Setting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, boolean, json
    public string Description { get; set; } = "";
    public bool IsPublic { get; set; } = false; // Can be accessed without authentication
}
