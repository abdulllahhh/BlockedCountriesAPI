namespace Business.Record
{
    public record IpCheckLog(
        string Ip,
        string? CountryCode,
        string? CountryName,
        bool IsBlocked,
        string? UserAgent,
        DateTime Timestamp
    );
}
