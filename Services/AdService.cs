using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Services
{
    /// <summary>
    /// Implements the logic for automatically selecting ads and recording statistics.
    /// </summary>
    public class AdService : IAdService
    {
        private readonly AppDbContext _context;

        public AdService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Selects a random eligible ad that can be displayed globally.
        /// </summary>
        public async Task<Ad?> GetRandomEligibleAdAsync(string? hostDomain = null)
        {
            var now = DateTime.UtcNow;

            // Step 1: Fetch only active and approved ads within date range.
            var query = _context.Ads
                .Where(a => a.Status == AdStatus.Approved)
                .Where(a => a.StartDate <= now)
                .Where(a => a.EndDate == null || a.EndDate >= now)
                .Where(a => a.Advertiser.Balance > 0);

            // Step 2: Optional filtering by category or domain (future use).
            // (We'll keep this line ready for future targeting support.)
            // if (!string.IsNullOrEmpty(hostDomain))
            //     query = query.Where(a => a.Category == "example");

            // Step 3: Filter out ads that exceeded max impressions or clicks.
            query = query.Where(a =>
                (!a.MaxImpressions.HasValue || a.Impressions!.Count < a.MaxImpressions.Value) &&
                (!a.MaxClicks.HasValue || a.Clicks!.Count < a.MaxClicks.Value)
            );

            // Step 4: Randomize and select one ad.
            var adCount = await query.CountAsync();
            if (adCount == 0)
                return null;

            var random = new Random();
            int skip = random.Next(0, adCount);
            return await query.Skip(skip).Take(1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Records a single impression (ad view) for tracking.
        /// </summary>
        public async Task RecordImpressionAsync(int adId, string? hostDomain, string? ip, string? userAgent)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return;

            var impression = new AdImpression
            {
                AdId = adId,
                HostDomain = hostDomain,
                IPAddress = ip,
                UserAgent = userAgent,
                ViewedAt = DateTime.UtcNow
            };

            await _context.AdImpressions.AddAsync(impression);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Records a single click for tracking and analytics.
        /// </summary>
        public async Task RecordClickAsync(int adId, string? hostDomain, string? ip, string? userAgent)
        {
            var ad = await _context.Ads.FindAsync(adId);
            if (ad == null) return;

            var click = new AdClick
            {
                AdId = adId,
                HostDomain = hostDomain,
                IPAddress = ip,
                UserAgent = userAgent,
                ClickedAt = DateTime.UtcNow
            };

            await _context.AdClicks.AddAsync(click);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Returns the advertiser's target URL for a specific ad.
        /// Used after a click to redirect the visitor.
        /// </summary>
        public async Task<string?> GetAdTargetUrlAsync(int adId)
        {
            return await _context.Ads
                .Where(a => a.Id == adId)
                .Select(a => a.TargetUrl)
                .FirstOrDefaultAsync();
        }
    }
}
