using AdSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdManagementSystem.Models
{
    public class AdPlacement
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ZoneKey { get; set; } = Guid.NewGuid().ToString("N");

        public int BannerSizeId { get; set; }
        public BannerSize? BannerSize { get; set; }

        public int WebsiteId { get; set; }

        [ForeignKey(nameof(WebsiteId))]
        public Website? Website { get; set; }
    }
}
