using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed class RefreshHandler : IRequestHandler<RefreshCommand, Result<RefreshResult>>
{
    private readonly IAuthService _auth;
    public RefreshHandler(IAuthService auth) => _auth = auth;

    public async Task<Result<RefreshResult>> Handle(RefreshCommand request, CancellationToken ct)
    {
        try
        {
            var audit = new AuthAuditContext(request.Ip, request.UserAgent);

            var res = await _auth.RefreshAsync(request.RefreshTokenRaw, audit, ct);
            return Result<RefreshResult>.Ok(res);
        }
        catch (UnauthorizedAuthException ex)
        {
            return Result<RefreshResult>.Fail(ResultError.Unauthorized(ex.Message));
        }
    }
}
