using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? Ip,
    string? UserAgent
) : IRequest<Result<LoginResult>>;