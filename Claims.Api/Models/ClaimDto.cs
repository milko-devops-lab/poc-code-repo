namespace Claims.Api.Models;

public record ClaimDto(
    int Id,
    string Customer,
    decimal Amount,
    string Status,
    DateTime CreatedAtUtc
);