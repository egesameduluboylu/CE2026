namespace BuildingBlocks.Web.Tenancy;

internal sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
}
