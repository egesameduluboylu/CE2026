using BuildingBlocks.Abstractions;
using MediatR;

namespace Modules.Identity.Application.Auth;

public sealed record ResetPasswordCommand(string Token, string NewPassword, string? Ip, string? UserAgent) : IRequest<Result>;
