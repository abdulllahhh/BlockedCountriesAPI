using Business.Interfaces;
using Business.Record;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class BlockedCountriesStore : IBlockedCountriesStore
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _countries = new();

        public bool Add(string code, string? name = null)
        {
            var upper = code.ToUpperInvariant();
            var country = new BlockedCountry(upper, name, DateTime.UtcNow, false, null);

            if (_countries.TryAdd(upper, country))
                return true;

            // If entry exists but is expired, overwrite it
            if (_countries.TryGetValue(upper, out var existing) && IsExpired(existing))
            {
                return _countries.TryUpdate(upper, country, existing);
            }

            return false;
        }

        public bool AddTemporary(string code, string? name, TimeSpan duration)
        {
            var upper = code.ToUpperInvariant();
            var expires = DateTime.UtcNow.Add(duration);
            var country = new BlockedCountry(upper, name, DateTime.UtcNow, true, expires);

            if (_countries.TryAdd(upper, country))
                return true;

            if (_countries.TryGetValue(upper, out var existing) && IsExpired(existing))
            {
                return _countries.TryUpdate(upper, country, existing);
            }

            return false;
        }

        public bool Remove(string code)
            => _countries.TryRemove(code.ToUpperInvariant(), out _);

        public bool IsBlocked(string code)
        {
            if (_countries.TryGetValue(code.ToUpperInvariant(), out var country))
            {
                return !IsExpired(country);
            }
            return false;
        }

        public IEnumerable<BlockedCountry> GetAll()
            => _countries.Values.Where(c => !IsExpired(c)).OrderBy(c => c.Code);

        public void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _countries)
            {
                if (IsExpired(kvp.Value))
                {
                    _countries.TryRemove(kvp.Key, out _);
                }
            }
        }

        private static bool IsExpired(BlockedCountry country)
        {
            return country.IsTemporary && 
                   country.ExpiresAt.HasValue && 
                   country.ExpiresAt.Value <= DateTime.UtcNow;
        }
    }
}

