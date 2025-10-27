using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdSystem.Models
{
    //public class AdClick
    //{
    //    public int Id { get; set; }

    //    public int AdId { get; set; }
    //    public Ad Ad { get; set; }

    //    // NEW: Which website generated the click
    //    public int WebsiteId { get; set; }
    //    public Website Website { get; set; }

    //    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    //    public string? ClickerIp { get; set; }
    //}
    /// <summary>
    /// Represents a record of a single ad click.
    /// </summary>
    public class AdClick
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Ad))]
        public int AdId { get; set; }
        public Ad? Ad { get; set; }

        /// <summary>
        /// Optional domain where the click originated.
        /// </summary>
        [ForeignKey(nameof(Website))]
        public int WebsiteId { get; set; }
        public Website? Website { get; set; }
        [StringLength(300)]
        public string? HostDomain { get; set; }

        [StringLength(45)]
        public string? IPAddress { get; set; }

        [StringLength(512)]
        public string? UserAgent { get; set; }

        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    }
}
