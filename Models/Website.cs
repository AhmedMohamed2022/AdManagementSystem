using AdManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AdSystem.Models
{
    //public class Website
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    public string Name { get; set; }

    //    [Required]
    //    public string Url { get; set; }

    //    // Auto-generated script key (publisher cannot edit it)
    //    public string ScriptKey { get; set; }

    //    // Admin must approve website before ads appear
    //    public bool IsApproved { get; set; } = false;

    //    public string? UserId { get; set; }
    //    public ApplicationUser User { get; set; }

    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    //}
    // <summary>
    /// Represents a publisher’s website that can display ads.
    /// </summary>
    public class Website
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(300)]
        //[Url]
        [RegularExpression(@"^(https?:\/\/)([a-zA-Z0-9\.\-]+|\blocalhost\b|\b127\.0\.0\.1\b)(:[0-9]+)?(\/.*)?$",
    ErrorMessage = "Please enter a valid URL including http:// or https://")]
        public string Domain { get; set; } = string.Empty;

       
        public string OwnerId { get; set; } = string.Empty; // ApplicationUser ID

        /// <summary>
        /// Public unique key used to embed the script in publisher sites.
        /// </summary>
        [StringLength(64)]
        public string? ScriptKey { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Indicates whether this website has been approved by the admin.
        /// Only approved sites can serve ads.
        /// </summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Optional category or description for targeting in the future.
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Date when the website was submitted/added.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<AdPlacement> ?Placements { get; set; }

    }
}
