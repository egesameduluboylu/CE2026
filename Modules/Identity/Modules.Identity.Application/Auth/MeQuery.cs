using BuildingBlocks.Abstractions;
using MediatR;
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth;

public sealed record MeQuery(
    string UserId,
    string? Email
) : IRequest<Result<MeResponse>>;