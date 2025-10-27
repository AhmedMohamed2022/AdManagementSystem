using AdSystem.Data;
using AdSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdManagementSystem.Controllers
{
    [Authorize]
    public class AdsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //GET: Ads
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var query = _context.Ads.AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admin sees all ads
                return View(await query.ToListAsync());
            }

            if (User.IsInRole("Advertiser"))
            {
                // Advertiser sees only their ads
                query = query.Where(a => a.AdvertiserId == user.Id);
                return View(await query.ToListAsync());
            }
            
            // Publishers have no direct ad management access
            return Forbid();
        }

        // GET: Ads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Ads.FirstOrDefaultAsync(m => m.Id == id);

            if (ad == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin") || ad.AdvertiserId == user.Id)
                return View(ad);

            return Forbid();
        }

        // GET: Ads/Create
        //[Authorize(Roles = "Advertiser,Admin")]
        //public IActionResult Create()
        //{
        //    // Admin can assign ads to any website, advertiser limited later
        //    ViewData["WebsiteId"] = new SelectList(_context.Websites, "Id", "Name");
        //    return View();
        //}
        [Authorize(Roles = "Advertiser,Admin")]
        public IActionResult Create()
        {
            return View();
        }


        //// POST: Ads/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Advertiser,Admin")]
        //public async Task<IActionResult> Create([Bind("Id,Title,ImageUrl,TargetUrl,IsActive,WebsiteId")] Ad ad)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    if (ModelState.IsValid)
        //    {
        //        ad.AdvertiserId = user.Id; // Link ad to current advertiser
        //        _context.Add(ad);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["WebsiteId"] = new SelectList(_context.Websites, "Id", "Name", ad.WebsiteId);
        //    return View(ad);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Advertiser,Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,ImageUrl,TargetUrl,IsActive")] Ad ad)
        {
            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                ad.AdvertiserId = user.Id;
                _context.Add(ad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ad);
        }


        // GET: Ads/Edit/5
        [HttpGet]
        [Authorize(Roles = "Advertiser,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && ad.AdvertiserId != user.Id)
                return Forbid();

            ViewData["WebsiteId"] = new SelectList(_context.Websites, "Id", "Name");
            return View(ad);
        }

        // POST: Ads/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Advertiser,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImageUrl,TargetUrl,IsActive,WebsiteId")] Ad ad)
        {
            if (id != ad.Id) return NotFound();

            var existingAd = await _context.Ads.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingAd == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && existingAd.AdvertiserId != user.Id)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    ad.AdvertiserId = existingAd.AdvertiserId; // Preserve ownership
                    _context.Update(ad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ads.Any(e => e.Id == ad.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["WebsiteId"] = new SelectList(_context.Websites, "Id", "Name");
            return View(ad);
        }

        // GET: Ads/Delete/5
        [Authorize(Roles = "Advertiser,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Ads
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && ad.AdvertiserId != user.Id)
                return Forbid();

            return View(ad);
        }

        // POST: Ads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Advertiser,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && ad.AdvertiserId != user.Id)
                return Forbid();

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> TrackImpression(int adId)
        //{
        //    var ad = await _context.Ads.FindAsync(adId);
        //    if (ad == null) return NotFound();

        //    ad.Impressions++;
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> TrackClick(int adId)
        //{
        //    var ad = await _context.Ads.FindAsync(adId);
        //    if (ad == null) return NotFound();

        //    ad.Clicks++;
        //    await _context.SaveChangesAsync();

        //    // Optional: redirect to the ad target
        //    return Redirect(ad.TargetUrl);
        //}

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> GetAdStats()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var query = _context.Ads.AsQueryable();

        //    if (User.IsInRole("Advertiser"))
        //        query = query.Where(a => a.AdvertiserId == user.Id);

        //    // ⚙️ Execute SQL-compatible grouping first
        //    var statsRaw = await query
        //        .GroupBy(a => a.CreatedAt.Date)
        //        .Select(g => new
        //        {
        //            Date = g.Key, // store as DateTime
        //            Clicks = g.Sum(a => a.Clicks),
        //            Impressions = g.Sum(a => a.Impressions)
        //        })
        //        .OrderBy(g => g.Date)
        //        .ToListAsync();

        //    // ✅ Format Date AFTER data is fetched (in memory)
        //    var stats = statsRaw.Select(g => new
        //    {
        //        Date = g.Date.ToString("yyyy-MM-dd"),
        //        g.Clicks,
        //        g.Impressions
        //    });

        //    return Json(stats);
        //}



        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Admin()
        //{
        //    var ads = await _context.Ads
        //        .Include(a => a.Advertiser)
        //        .Select(a => new
        //        {
        //            a.Id,
        //            a.Title,
        //            a.TargetUrl,
        //            a.Impressions,
        //            a.Clicks,
        //            CTR = a.Impressions > 0 ? Math.Round((double)a.Clicks / a.Impressions * 100, 2) : 0
        //        })
        //        .ToListAsync();

        //    return View(ads);
        //}

    }
}
