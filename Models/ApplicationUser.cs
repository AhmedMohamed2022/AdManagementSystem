using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

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
    }
}
