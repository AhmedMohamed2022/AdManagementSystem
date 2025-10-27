using AdManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AdSystem.Models
{
    //public class Ad
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    [Display(Name = "عنوان الإعلان")]
    //    public string Title { get; set; }

    //    public string? ImageUrl { get; set; }

    //    [Display(Name = "رابط الهدف")]
    //    public string? TargetUrl { get; set; }

    //    public bool IsActive { get; set; } = true;

    //    // ✅ Link to Advertiser (creator)
    //    public string? AdvertiserId { get; set; }
    //    public ApplicationUser? Advertiser { get; set; }

    //    public int Impressions { get; set; } = 0;
    //    public int Clicks { get; set; } = 0;
    //    [DataType(DataType.DateTime)]
    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //}
    /// <summary>
    /// Represents an advertisement created by an advertiser to be shown globally.
    /// </summary>
    public class Ad
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Url]
        public string TargetUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional text or caption displayed with the ad.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// The advertiser’s user ID (ApplicationUser.Id).
        /// </summary>
        public string AdvertiserId { get; set; } = string.Empty;

        /// <summary>
        /// Current review or display status of the ad.
        /// </summary>
        [Required]
        public AdStatus Status { get; set; } = AdStatus.Pending;

        /// <summary>
        /// Optional targeting category to match website categories in future extensions.
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// The date range during which the ad should be active.
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Max allowed views (impressions) or clicks before pausing automatically.
        /// </summary>
        public int? MaxImpressions { get; set; }
        public int? MaxClicks { get; set; }

        /// <summary>
        /// Related tracking entries (performance logs).
        /// </summary>
        public ICollection<AdImpression>? Impressions { get; set; }
        public ICollection<AdClick>? Clicks { get; set; }
    }
}
