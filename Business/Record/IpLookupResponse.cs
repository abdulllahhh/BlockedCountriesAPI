namespace Business.Record
{
    public record IpLookupResponse(
        string Ip,
        string? CountryCode,
        string? CountryName,
        string? Isp);
}
