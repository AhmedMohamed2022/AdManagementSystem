using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
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
