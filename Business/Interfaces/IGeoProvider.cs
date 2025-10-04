using Business.Models;

namespace Business.Interfaces
{
    public interface IGeoProvider
    {
        Task<GeoResult?> LookupIpAsync(string ip, CancellationToken cancellationToken = default);

    }
}
