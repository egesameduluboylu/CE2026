using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Infrastructure.Http;

public sealed class HttpClientDefaultsOptions
{
    [Range(1, 600)]
    public int TimeoutSeconds { get; init; } = 30;

    public bool ApplyToAll { get; init; } = false;
}
