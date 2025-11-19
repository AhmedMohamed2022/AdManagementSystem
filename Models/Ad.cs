using AdManagementSystem.Models;
using AdManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace AdSystem.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? ImagePath { get; set; } = string.Empty;

        [Required, Url]
        public string TargetUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string AdvertiserId { get; set; } = string.Empty;
        public ApplicationUser? Advertiser { get; set; }

        [Required]
        public AdStatus Status { get; set; } = AdStatus.Pending;
        /// <summary>
        /// Optional targeting category to match website categories in future extensions.
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }
        public int BannerSizeId { get; set; }
        public BannerSize? BannerSize { get; set; }

        // ✅ New scheduling
        public DateTime? StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        // ✅ New limits
        public int? MaxImpressions { get; set; }
        public int? MaxClicks { get; set; }

        public ICollection<AdImpression>? Impressions { get; set; }
        public ICollection<AdClick>? Clicks { get; set; }

    }
}
