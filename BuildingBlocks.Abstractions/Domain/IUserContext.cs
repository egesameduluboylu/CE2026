namespace BuildingBlocks.Abstractions.Domain
{
    public interface IUserContext
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        string? IpAddress { get; }
    }
}
