using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed record RefreshCommand(
    string RefreshTokenRaw,
    string? Ip,
    string? UserAgent
) : IRequest<Result<RefreshResult>>;