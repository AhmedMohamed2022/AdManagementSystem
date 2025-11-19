using AdSystem.Data;
using AdSystem.Models;
using AdSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class PublisherWalletController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFinanceService _financeService;

        public PublisherWalletController(AppDbContext db, UserManager<ApplicationUser> userManager, IFinanceService financeService)
        {
            _db = db;
            _userManager = userManager;
            _financeService = financeService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var transactions = await _financeService.GetTransactionsForUserAsync(user.Id);
            ViewBag.Transactions = transactions;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestWithdrawal(decimal amount, string? payoutDetails)
        {
            var user = await _userManager.GetUserAsync(User);
            try
            {
                await _financeService.RequestWithdrawalAsync(user.Id, amount, payoutDetails);
                TempData["Success"] = "Withdrawal processed automatically.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
