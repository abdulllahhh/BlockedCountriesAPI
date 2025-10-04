namespace Business.Record
{
    public record BlockedCountry(string Code, string? Name, DateTime CreatedAt, bool IsTemporary = false, DateTime? ExpiresAt = null);

}
