using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdSystem.Models
{
    //public class AdImpression
    //{
    //    public int Id { get; set; }

    //    public int AdId { get; set; }
    //    public Ad Ad { get; set; }

    //    // NEW: Which website generated the impression
    //    public int WebsiteId { get; set; }
    //    public Website Website { get; set; }

    //    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    //    public string? ViewerIp { get; set; }
    //}
    /// <summary>
    /// Represents a record of a single ad view (impression).
    /// </summary>
    public class AdImpression
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Ad))]
        public int AdId { get; set; }
        public Ad? Ad { get; set; }

        /// <summary>
        /// Optional domain or host where the ad was displayed (helps prevent fraud).
        /// </summary>
        /// 
        [ForeignKey(nameof(Website))]
        public int WebsiteId { get; set; }
        public Website? Website { get; set; }
        [StringLength(300)]
        public string? HostDomain { get; set; }

        [StringLength(45)]
        public string? IPAddress { get; set; }

        [StringLength(512)]
        public string? UserAgent { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
