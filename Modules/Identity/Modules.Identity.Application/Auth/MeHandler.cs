using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed class MeHandler : IRequestHandler<MeQuery, Result<MeResponse>>
{
    public Task<Result<MeResponse>> Handle(MeQuery request, CancellationToken ct)
    {
        return Task.FromResult(Result<MeResponse>.Ok(new MeResponse(request.UserId, request.Email)));
    }
}
