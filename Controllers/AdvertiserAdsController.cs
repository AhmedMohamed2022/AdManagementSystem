using AdManagementSystem.Data;
using AdManagementSystem.Models;
using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Advertiser")]
    public class AdvertiserAdsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdvertiserAdsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //// ==========================
        //// 🔹 GET: /AdvertiserAds
        //// ==========================
        //public async Task<IActionResult> Index()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var ads = _context.Ads
        //        .Where(a => a.AdvertiserId == user.Id)
        //        .OrderByDescending(a => a.Id)
        //        .ToList();

        //    return View(ads);
        //}
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var ads = await _context.Ads
                .Where(a => a.AdvertiserId == user.Id)
                .Select(a => new AdvertiserAdViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Status = a.Status,
                    Impressions = _context.AdImpressions.Count(i => i.AdId == a.Id),
                    Clicks = _context.AdClicks.Count(c => c.AdId == a.Id)
                })
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            return View(ads);
        }

        // ==========================
        // 🔹 GET: /AdvertiserAds/Create
        // ==========================
        public IActionResult Create() => View();

        // ==========================
        // 🔹 POST: /AdvertiserAds/Create
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ad ad)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(ad);

            ad.AdvertiserId = user.Id;
            ad.Status = AdStatus.Pending; // Admin must approve (optional future rule)
            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // ==========================
        // 🔹 GET: /AdvertiserAds/Edit/5
        // ==========================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        // ==========================
        // 🔹 POST: /AdvertiserAds/Edit/5
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ad ad)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return View(ad);

            existing.Title = ad.Title;
            existing.Description = ad.Description;
            existing.ImageUrl = ad.ImageUrl;
            existing.TargetUrl = ad.TargetUrl;
            existing.Status = ad.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // 🔹 GET: /AdvertiserAds/Delete/5
        // ==========================
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        // ==========================
        // 🔹 POST: /AdvertiserAds/DeleteConfirmed/5
        // ==========================
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
            if (ad == null) return NotFound();

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
