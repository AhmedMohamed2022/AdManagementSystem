using System.ComponentModel.DataAnnotations;

namespace AdManagementSystem.Models
{
    public class BannerSize
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }
    }

}
