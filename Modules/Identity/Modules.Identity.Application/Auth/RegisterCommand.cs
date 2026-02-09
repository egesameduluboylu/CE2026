using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public record RegisterCommand(
    string Email,
    string Password,
    string? Ip,
    string? UserAgent
) : IRequest<Result<RegisterResponse>>;