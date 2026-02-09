using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
