using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Result<Unit>>
{
    private readonly IAuthService _auth;
    public LogoutHandler(IAuthService auth) => _auth = auth;

    public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken ct)
    {
        var audit = new AuthAuditContext(request.Ip, request.UserAgent);

        await _auth.LogoutAsync(request.RefreshTokenRaw, audit, ct);
        return Result<Unit>.Ok(Unit.Value);
    }
}
