using AdSystem.Data;
using AdSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdSystem.Services
{
    public class AdPricingService : IAdPricingService
    {
        private readonly AppDbContext _db;

        public AdPricingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdPricingRule>> GetAllAsync()
        {
            return await _db.AdPricingRules.OrderBy(r => r.RuleType).ToListAsync();
        }

        public async Task<AdPricingRule?> GetByIdAsync(int id)
        {
            return await _db.AdPricingRules.FindAsync(id);
        }

        public async Task CreateAsync(AdPricingRule rule)
        {
            _db.AdPricingRules.Add(rule);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(AdPricingRule rule)
        {
            rule.UpdatedAt = DateTime.UtcNow;
            _db.AdPricingRules.Update(rule);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.AdPricingRules.FindAsync(id);
            if (entity != null)
            {
                _db.AdPricingRules.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Pricing priority: Advertiser → City → Country → Global
        /// </summary>
        public async Task<AdPricingRule> GetEffectivePricingAsync(string? advertiserId, string? country, string? city)
        {
            // 1) Advertiser-specific rule
            if (!string.IsNullOrEmpty(advertiserId))
            {
                var custom = await _db.AdPricingRules
                    .Where(r => r.RuleType == "Advertiser" && r.AdvertiserId == advertiserId)
                    .FirstOrDefaultAsync();
                if (custom != null) return custom;
            }

            // 2) City rule
            if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(city))
            {
                var cityRule = await _db.AdPricingRules
                    .Where(r => r.RuleType == "City" && r.Country == country && r.City == city)
                    .FirstOrDefaultAsync();
                if (cityRule != null) return cityRule;
            }

            // 3) Country rule
            if (!string.IsNullOrEmpty(country))
            {
                var countryRule = await _db.AdPricingRules
                    .Where(r => r.RuleType == "Country" && r.Country == country && r.City == null)
                    .FirstOrDefaultAsync();
                if (countryRule != null) return countryRule;
            }

            // 4) Global fallback
            var global = await _db.AdPricingRules
                .Where(r => r.RuleType == "Global")
                .FirstOrDefaultAsync();

            if (global == null)
            {
                // Create automatic fallback if admin forgot
                global = new AdPricingRule
                {
                    RuleType = "Global",
                    CPM = 10,
                    CPC = 0.5M
                };
                _db.AdPricingRules.Add(global);
                await _db.SaveChangesAsync();
            }

            return global;
        }
    }
}
