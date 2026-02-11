using BuildingBlocks.Abstractions.Domain;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Services
{
    public class UserContextService : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value?.ToString();
        public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value?.ToString();
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }
}
