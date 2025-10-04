namespace Business.Record
{
    public record BlockedCountryResponse(
        string Code,
        string? Name,
        bool IsTemporary,
        DateTime? ExpiresAt
    );
}
