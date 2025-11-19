using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

//namespace AdSystem.Services
//{
//    public class GeoLocationService : IGeoLocationService
//    {
//        private readonly HttpClient _http;
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<GeoLocationService> _logger;

//        // Cache TTL for resolved IP lookups to reduce API calls
//        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

//        public GeoLocationService(HttpClient http, IMemoryCache cache, ILogger<GeoLocationService> logger)
//        {
//            _http = http;
//            _cache = cache;
//            _logger = logger;
//        }

//        public async Task<(string? Country, string? City)> ResolveIpAsync(string? ip)
//        {
//            if (string.IsNullOrWhiteSpace(ip))
//                return (null, null);

//            // Normalize IPv6 mapped IPv4
//            if (ip.StartsWith("::ffff:"))
//                ip = ip.Substring("::ffff:".Length);

//            var cacheKey = $"geo:{ip}";

//            if (_cache.TryGetValue(cacheKey, out (string? Country, string? City) cached))
//            {
//                return cached;
//            }

//            try
//            {
//                // ipapi.co free endpoint (no API key required for basic usage)
//                var url = $"https://ipapi.co/{ip}/json/";
//                //var url = $"http://ip-api.com/json/{ip}?fields=status,country,city";

//                using var resp = await _http.GetAsync(url);
//                if (!resp.IsSuccessStatusCode)
//                {
//                    _logger.LogDebug("Geo lookup failed for IP {IP}. Status: {Status}", ip, resp.StatusCode);
//                    _cache.Set<(string?, string?)>(cacheKey, (null, null), DateTimeOffset.UtcNow.AddMinutes(5)); // short cache negative
//                    return (null, null);
//                }

//                var str = await resp.Content.ReadAsStringAsync();
//                if (string.IsNullOrWhiteSpace(str))
//                {
//                    _cache.Set<(string?, string?)>(cacheKey, (null, null), DateTimeOffset.UtcNow.AddMinutes(5));
//                    return (null, null);
//                }

//                using var doc = JsonDocument.Parse(str);
//                var root = doc.RootElement;

//                string? country = null;
//                string? city = null;

//                // ipapi.co returns "country_name", "city"
//                if (root.TryGetProperty("country_name", out var countryEl))
//                    country = countryEl.GetString();

//                if (root.TryGetProperty("city", out var cityEl))
//                    city = cityEl.GetString();

//                // Normalize blanks
//                if (string.IsNullOrWhiteSpace(country)) country = null;
//                if (string.IsNullOrWhiteSpace(city)) city = null;

//                var result = (Country: country, City: city);
//                _cache.Set<(string?, string?)>(cacheKey, result, DateTimeOffset.UtcNow.Add(CacheDuration));
//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning(ex, "Geo lookup exception for IP {IP}", ip);
//                _cache.Set<(string?, string?)>(cacheKey, (null, null), DateTimeOffset.UtcNow.AddMinutes(5));
//                return (null, null);
//            }
//        }
//    }
//}
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdSystem.Services
{
    public class GeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GeoLocationService> _logger;

        // Cache TTL for resolved IP lookups to reduce API calls
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);
        private static readonly TimeSpan NegativeCacheDuration = TimeSpan.FromMinutes(30);

        public GeoLocationService(HttpClient http, IMemoryCache cache, ILogger<GeoLocationService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(string? Country, string? City)> ResolveIpAsync(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return (null, null);

            // Normalize IPv6 mapped IPv4
            if (ip.StartsWith("::ffff:"))
                ip = ip.Substring("::ffff:".Length);

            var cacheKey = $"geo:{ip}";

            if (_cache.TryGetValue(cacheKey, out (string? Country, string? City) cached))
            {
                return cached;
            }

            try
            {
                // ip-api.com free endpoint (no API key required for basic usage)
                // Note: https is a paid feature for ip-api; use http for the free tier.
                var url = $"http://ip-api.com/json/{ip}?fields=status,country,city,message";

                using var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Geo lookup HTTP failure for IP {IP}. Status: {StatusCode}", ip, resp.StatusCode);
                    _cache.Set<(string?, string?)>(cacheKey, (null, null), NegativeCacheDuration); // short cache negative
                    return (null, null);
                }

                var str = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(str))
                {
                    _logger.LogDebug("Geo lookup returned empty body for IP {IP}.", ip);
                    _cache.Set<(string?, string?)>(cacheKey, (null, null), NegativeCacheDuration);
                    return (null, null);
                }

                using var doc = JsonDocument.Parse(str);
                var root = doc.RootElement;

                // ip-api returns { "status": "success"|"fail", "country": "...", "city":"...", "message":"..." }
                if (root.TryGetProperty("status", out var statusEl))
                {
                    var status = statusEl.GetString();
                    if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        var msg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
                        _logger.LogDebug("Geo lookup failed for IP {IP}. provider status={Status}, message={Message}", ip, status, msg);
                        _cache.Set<(string?, string?)>(cacheKey, (null, null), NegativeCacheDuration);
                        return (null, null);
                    }
                }
                else
                {
                    _logger.LogDebug("Geo lookup response missing 'status' for IP {IP}: {Json}", ip, str);
                    _cache.Set<(string?, string?)>(cacheKey, (null, null), NegativeCacheDuration);
                    return (null, null);
                }

                string? country = null;
                string? city = null;

                if (root.TryGetProperty("country", out var countryEl) && countryEl.ValueKind != JsonValueKind.Null)
                    country = countryEl.GetString();

                if (root.TryGetProperty("city", out var cityEl) && cityEl.ValueKind != JsonValueKind.Null)
                    city = cityEl.GetString();

                // Normalize blanks
                if (string.IsNullOrWhiteSpace(country)) country = null;
                if (string.IsNullOrWhiteSpace(city)) city = null;

                var result = (Country: country, City: city);
                _cache.Set(cacheKey, result, CacheDuration);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geo lookup exception for IP {IP}", ip);
                _cache.Set<(string?, string?)>(cacheKey, (null, null), NegativeCacheDuration);
                return (null, null);
            }
        }
    }
}

//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;
//using MaxMind.GeoIP2;
//using MaxMind.GeoIP2.Exceptions;
//using System;
//using System.Threading.Tasks;

//namespace AdSystem.Services
//{
//    public class GeoLocationService : IGeoLocationService
//    {
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<GeoLocationService> _logger;

//        // GeoLite DB full path
//        private const string DbPath = @"D:\AdManagementSystemProject\AdManagementSystem\GeoLite2-City.mmdb";

//        // Cache TTL for resolved IP lookups
//        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

//        public GeoLocationService(IMemoryCache cache, ILogger<GeoLocationService> logger)
//        {
//            _cache = cache;
//            _logger = logger;
//        }

//        public async Task<(string? Country, string? City)> ResolveIpAsync(string? ip)
//        {
//            if (string.IsNullOrWhiteSpace(ip))
//                return (null, null);

//            // Normalize IPv6 mapped IPv4
//            if (ip.StartsWith("::ffff:"))
//                ip = ip.Substring("::ffff:".Length);

//            var cacheKey = $"geo:{ip}";

//            if (_cache.TryGetValue(cacheKey, out (string? Country, string? City) cached))
//                return cached;

//            try
//            {
//                using var reader = new DatabaseReader(DbPath);

//                var cityResponse = reader.City(ip);

//                string? country = cityResponse?.Country?.Name;
//                string? city = cityResponse?.City?.Name;

//                if (string.IsNullOrWhiteSpace(country)) country = null;
//                if (string.IsNullOrWhiteSpace(city)) city = null;

//                var result = (Country: country, City: city);

//                _cache.Set(cacheKey, result, CacheDuration);

//                return result;
//            }
//            catch (AddressNotFoundException)
//            {
//                _cache.Set<(string?, string?)>(cacheKey, (null, null), TimeSpan.FromMinutes(30));
//                return (null, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning(ex, "MaxMind geo lookup error for IP {IP}", ip);
//                _cache.Set<(string?, string?)>(cacheKey, (null, null), TimeSpan.FromMinutes(10));
//                return (null, null);
//            }
//        }
//    }
//}
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using MaxMind.GeoIP2;
//using MaxMind.GeoIP2.Exceptions;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using AdManagementSystem.Options;

//namespace AdSystem.Services
//{
//    public class GeoLocationService : IGeoLocationService
//    {
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<GeoLocationService> _logger;
//        private readonly string _dbFullPath;

//        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

//        public GeoLocationService(
//            IMemoryCache cache,
//            ILogger<GeoLocationService> logger,
//            IOptions<GeoIPOptions> geoOptions)
//        {
//            _cache = cache;
//            _logger = logger;

//            // Resolve absolute DB path (supports production & development)
//            var dbPath = geoOptions.Value.DatabasePath;
//            _dbFullPath = Path.GetFullPath(dbPath);

//            if (!File.Exists(_dbFullPath))
//            {
//                logger.LogError("MaxMind GeoLite2 database NOT FOUND at {Path}", _dbFullPath);
//            }
//        }

//        public async Task<(string? Country, string? City)> ResolveIpAsync(string? ip)
//        {
//            if (string.IsNullOrWhiteSpace(ip))
//                return (null, null);

//            // Normalize IPv6-mapped IPv4
//            if (ip.StartsWith("::ffff:"))
//                ip = ip.Substring("::ffff:".Length);

//            var cacheKey = $"geo:{ip}";

//            if (_cache.TryGetValue(cacheKey, out (string? Country, string? City) cached))
//                return cached;

//            try
//            {
//                if (!File.Exists(_dbFullPath))
//                {
//                    _logger.LogWarning("GeoIP lookup skipped. Database file missing: {Path}", _dbFullPath);
//                    return (null, null);
//                }

//                using var reader = new DatabaseReader(_dbFullPath);
//                var response = reader.City(ip);

//                string? country = response?.Country?.Name;
//                string? city = response?.City?.Name;

//                if (string.IsNullOrWhiteSpace(country)) country = null;
//                if (string.IsNullOrWhiteSpace(city)) city = null;

//                var result = (country, city);

//                _cache.Set(cacheKey, result, CacheDuration);

//                return result;
//            }
//            catch (AddressNotFoundException)
//            {
//                _cache.Set<(string?, string?)>(cacheKey, (null, null), TimeSpan.FromMinutes(30));
//                return (null, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "GeoIP lookup failed for IP {IP}", ip);
//                _cache.Set<(string?,string?)>(cacheKey, (null, null), TimeSpan.FromMinutes(10));
//                return (null, null);
//            }
//        }
//    }
//}
