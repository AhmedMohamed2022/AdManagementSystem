using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdSystem.Models
{
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
        /// ✅ Country resolved from IP or request
        [StringLength(100)]
        public string? Country { get; set; }

        /// ✅ City resolved from IP or request
        [StringLength(100)]
        public string? City { get; set; }

        /// ✅ Money earned for this single impression
        [Column(TypeName = "decimal(18,6)")]
        public decimal EarnedAmount { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
