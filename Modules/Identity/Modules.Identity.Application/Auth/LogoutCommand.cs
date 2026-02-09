using BuildingBlocks.Abstractions;
using MediatR;

namespace Modules.Identity.Application.Auth;

public sealed record LogoutCommand(
    string? RefreshTokenRaw,
    string? Ip,
    string? UserAgent
) : IRequest<Result<Unit>>;