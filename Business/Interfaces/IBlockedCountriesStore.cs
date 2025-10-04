using Business.Record;

namespace Business.Interfaces
{
    public interface IBlockedCountriesStore
    {
        bool Add(string code, string? name = null);
        bool AddTemporary(string code, string? name, TimeSpan duration);
        bool Remove(string code);
        bool IsBlocked(string code);
        IEnumerable<BlockedCountry> GetAll();
        void CleanupExpired();
    }
}
