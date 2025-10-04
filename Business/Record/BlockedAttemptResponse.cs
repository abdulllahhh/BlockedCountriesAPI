namespace Business.Record
{
    public record BlockedAttemptResponse(
        string Ip,
        string? CountryCode,
        string? CountryName,
        DateTime Timestamp,
        string? UserAgent
    );
}
