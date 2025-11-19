using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdSystem.Models
{
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
        /// ✅ Country resolved from IP or request
        [StringLength(100)]
        public string? Country { get; set; }

        /// ✅ City resolved from IP or request
        [StringLength(100)]
        public string? City { get; set; }

        /// ✅ Money earned for this click
        [Column(TypeName = "decimal(18,6)")]
        public decimal EarnedAmount { get; set; }


        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    }
}
