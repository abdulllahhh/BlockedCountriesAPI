using Business.Interfaces;
using Business.Record;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class BlockedCountriesStore : IBlockedCountriesStore
    {
        private readonly ConcurrentDictionary<string, bool> _blockedCountries = new();
        private readonly ConcurrentDictionary<string, BlockedCountry> _countries = new();

        public bool Add(string code, string? name = null)
        {
            var upper = code.ToUpperInvariant();
            var country = new BlockedCountry(upper, name, DateTime.UtcNow, false, null);
            return _countries.TryAdd(upper, country);
        }

        public bool AddTemporary(string code, string? name, TimeSpan duration)
        {
            var upper = code.ToUpperInvariant();
            var expires = DateTime.UtcNow.Add(duration);
            var country = new BlockedCountry(upper, name, DateTime.UtcNow, true, expires);
            return _countries.TryAdd(upper, country);
        }

        public bool Remove(string code)
            => _countries.TryRemove(code.ToUpperInvariant(), out _);

        public bool IsBlocked(string code)
        {
            if (_countries.TryGetValue(code.ToUpperInvariant(), out var country))
            {
                if (country.IsTemporary && country.ExpiresAt.HasValue && country.ExpiresAt.Value <= DateTime.UtcNow)
                {
                    _countries.TryRemove(code.ToUpperInvariant(), out _);
                    return false;
                }
                return true;
            }
            return false;
        }

        public IEnumerable<BlockedCountry> GetAll()
            => _countries.Values.OrderBy(c => c.Code);

        public void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _countries)
            {
                var country = kvp.Value;
                if (country.IsTemporary && country.ExpiresAt.HasValue && country.ExpiresAt.Value <= now)
                {
                    _countries.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

}
