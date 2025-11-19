using System.Threading.Tasks;

namespace AdSystem.Services
{
    public interface IGeoLocationService
    {
        /// <summary>
        /// Resolve an IP address to (Country, City).
        /// Returns (null, null) if resolution failed.
        /// </summary>
        Task<(string? Country, string? City)> ResolveIpAsync(string? ip);
    }
}
