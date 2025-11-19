using AdSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdManagementSystem.Models
{
    public class WithdrawalRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
        public ApplicationUser? User { get; set; }

        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Pending, Approved, Rejected
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public string? AdminNote { get; set; }
    }

}
