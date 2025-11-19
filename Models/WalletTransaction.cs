using AdManagementSystem.Models.Enums;
using AdSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdManagementSystem.Models
{
    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }

        public string? FromUserId { get; set; }
        public ApplicationUser? FromUser { get; set; }   // ✅ Add nav prop

        public string? ToUserId { get; set; }
        public ApplicationUser? ToUser { get; set; }     // ✅ Add nav prop

        public int? AdId { get; set; }
        public int? WebsiteId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
