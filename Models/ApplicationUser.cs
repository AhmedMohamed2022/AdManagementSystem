using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Role clarification:
        // Admin – manages everything
        // Advertiser – creates ads
        // Publisher – owns websites displaying ads

        public string? DisplayName { get; set; }

        // A Publisher can have many websites
        public ICollection<Website>? Websites { get; set; }

        // An Advertiser can have many ads
        public ICollection<Ad>? Ads { get; set; }
        // Wallet / balance (USD)
        // Use decimal(18,4) precision via DbContext mapping
        [Column(TypeName = "decimal(18,4)")]
        public decimal Balance { get; set; } = 0m;
        // Indicates if the user is active
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
