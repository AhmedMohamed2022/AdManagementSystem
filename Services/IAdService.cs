using AdSystem.Models;

namespace AdManagementSystem.Services
{
    /// <summary>
    /// Defines all operations related to automatic ad serving and tracking.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Returns a random eligible ad to serve for a given publisher website key or domain.
        /// </summary>
        /// <param name="hostDomain">Optional domain or identifier of the publisher site.</param>
        /// <returns>An eligible Ad object or null if none available.</returns>
        Task<Ad?> GetRandomEligibleAdAsync(string? hostDomain = null);

        /// <summary>
        /// Records a view (impression) for the given ad and host.
        /// </summary>
        Task RecordImpressionAsync(int adId, string? hostDomain, string? ip, string? userAgent);

        /// <summary>
        /// Records a click for the given ad and host.
        /// </summary>
        Task RecordClickAsync(int adId, string? hostDomain, string? ip, string? userAgent);
        /// <summary>
        /// Gets the target URL of a specific ad (for redirect after click).
        /// </summary>
        Task<string?> GetAdTargetUrlAsync(int adId);
    }
}
