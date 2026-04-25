namespace Business.Record
{
    public record CheckBlockResponse(
        string Ip,
        string? CountryCode,
        string? CountryName,
        bool IsBlocked);
}
