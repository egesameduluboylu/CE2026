using BuildingBlocks.Abstractions;
using MediatR;

namespace Modules.Identity.Application.Auth;

public sealed record ForgotPasswordCommand(string Email, string? Ip, string? UserAgent) : IRequest<Result>;
