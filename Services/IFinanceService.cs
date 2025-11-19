using AdManagementSystem.Models;
using AdSystem.Models;
using System.Threading.Tasks;

namespace AdSystem.Services
{
    public interface IFinanceService
    {
        Task HandleImpressionAsync(AdImpression impression);
        Task HandleClickAsync(AdClick click);

        // Admin manual adjustments (credit/debit)
        Task ManualAdjustAsync(string adminUserId, string userId, decimal amount, string description);

        // Admin deposit to advertiser (alias for ManualAdjust positive)
        Task DepositToAdvertiserAsync(string adminUserId, string advertiserId, decimal amount, string description);

        // Publisher requests withdrawal — processed automatically (no approval)
        Task RequestWithdrawalAsync(string publisherUserId, decimal amount, string? payoutDetails = null);

        // Query transactions for UI
        Task<List<WalletTransaction>> GetTransactionsForUserAsync(string userId, int take = 200);

         // ✅ NEW (auto-stop logic integration)
        Task<bool> HasEnoughBalanceForAdAsync(int adId);
        Task<bool> HasEnoughBalanceAsync(string userId, decimal required);
    }
}
