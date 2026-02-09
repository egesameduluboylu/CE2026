using MediatR;
using BuildingBlocks.Abstractions;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAuthService _auth;

    public RegisterHandler(IAuthService auth) => _auth = auth;

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = new RegisterRequest(request.Email, request.Password);

            var audit = new AuthAuditContext(request.Ip, request.UserAgent);

            var res = await _auth.RegisterAsync(req, audit, cancellationToken);

            return Result<RegisterResponse>.Ok(res);
        }
        catch (ConflictAuthException ex)
        {
            return Result<RegisterResponse>.Fail(ResultError.Conflict(ex.Message));
        }
    }
}
