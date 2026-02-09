using Modules.Identity.Contracts.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Application.Admin
{
    public interface IAdminUsersQuery
    {
        Task<PagedResult<AdminUserListItem>> GetUsersAsync(string? search, int page, int pageSize, CancellationToken ct);
        Task<AdminUserDetailResponse?> GetUserAsync(Guid id, CancellationToken ct);
        Task<RevokeTokensResponse> RevokeRefreshTokensAsync(Guid id, CancellationToken ct);
    }
}
