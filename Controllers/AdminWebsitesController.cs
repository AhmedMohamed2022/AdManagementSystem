using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdManagementSystem.Data;
using System.Linq;
using System.Threading.Tasks;
using AdSystem.Data;
using AdManagementSystem.Models.Enums;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminWebsitesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminWebsitesController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // 🔹 GET: /AdminWebsites
        // ==========================
        public IActionResult Index()
        {
            var sites = _context.Websites.OrderByDescending(w => w.CreatedAt).ToList();
            return View(sites);
        }
        // ==========================
        // 🔹 GET: /AdminAds
        // ==========================
        public IActionResult Ads()
        {
            var ads = _context.Ads.OrderByDescending(w => w.StartDate).ToList();
            return View(ads);
        }

        // ==========================
        // 🔹 POST: /AdminWebsites/Approve/5
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var site = await _context.Websites.FindAsync(id);
            if (site == null) return NotFound();

            site.IsApproved = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ApproveAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            ad.Status = AdStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Ads));
        }

        // ==========================
        // 🔹 POST: /AdminWebsites/Reject/5
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var site = await _context.Websites.FindAsync(id);
            if (site == null) return NotFound();
            site.IsApproved = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Ads));
        }
        [HttpPost]
        public async Task<IActionResult> RejectAd(int id)
        {
            var site = await _context.Ads.FindAsync(id);
            if (site == null) return NotFound();
            site.Status = AdStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Ads));
        }
        // 🔹 GET: /AdminWebsites/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var site = await _context.Websites.FindAsync(id);
            if (site == null) return NotFound();
            return View(site);
        }
        public async Task<IActionResult> AdDetails(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();
            return View(ad);
        }

    }
}
