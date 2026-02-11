namespace BuildingBlocks.Web.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantName { get; }
}
