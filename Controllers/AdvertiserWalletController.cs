using AdSystem.Data;
using AdSystem.Models;
using AdSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Advertiser")]
    public class AdvertiserWalletController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFinanceService _financeService;

        public AdvertiserWalletController(AppDbContext db, UserManager<ApplicationUser> userManager, IFinanceService financeService)
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

        // Advertiser cannot deposit directly — show message explaining admin-only deposit
        public IActionResult DepositInfo()
        {
            return View();
        }
    }
}
