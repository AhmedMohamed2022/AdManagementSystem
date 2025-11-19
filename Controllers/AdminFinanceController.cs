using AdSystem.Data;
using AdSystem.Models;
using AdSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFinanceController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFinanceService _financeService;

        public AdminFinanceController(AppDbContext db, UserManager<ApplicationUser> userManager, IFinanceService financeService)
        {
            _db = db;
            _userManager = userManager;
            _financeService = financeService;
        }

        // List advertisers (with balances)
        public async Task<IActionResult> Advertisers()
        {
            var advertisers = await _userManager.GetUsersInRoleAsync("Advertiser");
            var model = advertisers.Select(u => new
            {
                u.Id,
                u.Email,
                Balance = u.Balance
            }).ToList();
            ViewBag.Advertisers = model;
            return View(advertisers);
        }

        // GET: adjust form
        public async Task<IActionResult> AdjustBalance(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: perform adjust (positive = deposit to advertiser)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustBalancePost(string id, decimal amount, string description)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var adminId = (await _userManager.GetUserAsync(User))?.Id ?? "system";
            await _financeService.ManualAdjustAsync(adminId, id, amount, description ?? $"Admin adjustment {amount}");

            return RedirectToAction(nameof(Advertisers));
        }
        // GET: View advertiser details
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            // Include Ads in the query to show advertiser's ads
            var user = await _db.Users
                .Include(u => u.Ads)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Verify user is an advertiser
            var isAdvertiser = await _userManager.IsInRoleAsync(user, "Advertiser");
            if (!isAdvertiser) return BadRequest("User is not an advertiser");

            return View(user);
        }
        // View transactions (system-wide)
        public async Task<IActionResult> Transactions(
    string? search,
    DateTime? from,
    DateTime? to,
    int page = 1,
    int pageSize = 20)
        {
            var query = _db.WalletTransactions
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .AsQueryable();

            // Search by From or To emails
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    (t.FromUser != null && t.FromUser.Email.Contains(search)) ||
                    (t.ToUser != null && t.ToUser.Email.Contains(search)));
            }

            // Date filters
            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value.AddDays(1).AddSeconds(-1));

            // Summary Stats (based on filtered results)
            ViewBag.TotalCredits = await query
                .Where(t => t.Amount > 0)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            ViewBag.TotalDebits = await query
                .Where(t => t.Amount < 0)
                .SumAsync(t => (decimal?)(t.Amount * -1)) ?? 0;

            ViewBag.NetFlow = ViewBag.TotalCredits - ViewBag.TotalDebits;

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var tx = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(tx);
        }


        public async Task<IActionResult> ExportCsv()
        {
            var tx = await _db.WalletTransactions
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Type,Amount,FromUserId,ToUserId,AdId,WebsiteId,Description,CreatedAt");

            foreach (var t in tx)
            {
                sb.AppendLine($"{t.Id},{t.Type},{t.Amount},{t.FromUserId},{t.ToUserId},{t.AdId},{t.WebsiteId},\"{t.Description}\",{t.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"transactions_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var tx = await _db.WalletTransactions
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate());
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.AddCell("ID");
            table.AddCell("Type");
            table.AddCell("Amount");
            table.AddCell("From");
            table.AddCell("To");
            table.AddCell("Date");

            foreach (var t in tx)
            {
                table.AddCell(t.Id.ToString());
                table.AddCell(t.Type.ToString());
                table.AddCell(t.Amount.ToString("0.00"));
                table.AddCell(t.FromUserId ?? "-");
                table.AddCell(t.ToUserId ?? "-");
                table.AddCell(t.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", $"transactions_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}
