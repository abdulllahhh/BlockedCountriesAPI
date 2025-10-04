namespace Business.Models
{
    public class GeoResult
    {
        public string? Ip { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Isp { get; set; }
        public object? RawResponse { get; set; }
    }
}
