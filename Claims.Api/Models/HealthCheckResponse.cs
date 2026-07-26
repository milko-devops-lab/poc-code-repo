namespace Claims.Api.Models;

public record HealthCheckResponse(
    string Status,
    string TotalDuration,
    IReadOnlyDictionary<string, string> Entries
);