using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IAuthService _auth;
    public LoginHandler(IAuthService auth) => _auth = auth;

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken ct)
    {
        try
        {
            var req = new LoginRequest(request.Email, request.Password);
            var audit = new AuthAuditContext(request.Ip, request.UserAgent);

            var res = await _auth.LoginAsync(req, audit, ct);
            return Result<LoginResult>.Ok(res);
        }
        catch (LockedAuthException ex)
        {
            return Result<LoginResult>.Fail(ResultError.Locked(ex.Message, ex.LockedUntil));
        }
        catch (UnauthorizedAuthException ex)
        {
            return Result<LoginResult>.Fail(ResultError.Unauthorized(ex.Message));
        }
    }
}
