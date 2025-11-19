using System.ComponentModel.DataAnnotations;

namespace AdSystem.Models
{
    /// <summary>
    /// Represents pricing rules for CPM/CPC based on Global, Country, City or Advertiser-custom.
    /// </summary>
    public class AdPricingRule
    {
        [Key]
        public int Id { get; set; }

        // "Global", "Country", "City", "Advertiser"
        [Required]
        [StringLength(30)]
        public string RuleType { get; set; } = "Global";

        // Optional: if null → applies globally
        [StringLength(100)]
        public string? Country { get; set; }

        // Optional: if null → applies to full country
        [StringLength(100)]
        public string? City { get; set; }

        // Optional: advertiser override
        public string? AdvertiserId { get; set; }

        [Range(0.01, 10000)]
        public decimal CPM { get; set; }   // Cost per 1000 impressions

        [Range(0.01, 10000)]
        public decimal CPC { get; set; }   // Cost per click

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
