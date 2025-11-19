using AdSystem.Models;

namespace AdSystem.Services
{
    public interface IAdPricingService
    {
        Task<List<AdPricingRule>> GetAllAsync();
        Task<AdPricingRule?> GetByIdAsync(int id);
        Task CreateAsync(AdPricingRule rule);
        Task UpdateAsync(AdPricingRule rule);
        Task DeleteAsync(int id);

        // The pricing engine:
        Task<AdPricingRule> GetEffectivePricingAsync(string? advertiserId, string? country, string? city);
    }
}
