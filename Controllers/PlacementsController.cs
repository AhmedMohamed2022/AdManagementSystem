using AdManagementSystem.Data;
using AdManagementSystem.Models;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class PlacementsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlacementsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int websiteId)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Sizes = _context.BannerSizes.ToList();
            var website = await _context.Websites
                .Include(w => w.Placements)
                .FirstOrDefaultAsync(w => w.Id == websiteId && w.OwnerId == user.Id);

            if (website == null)
                return NotFound();

            return View(website);
        }

        [HttpGet]
        public IActionResult Create(int websiteId)
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();

            return View(new AdPlacement { WebsiteId = websiteId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdPlacement model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.ZoneKey = Guid.NewGuid().ToString("N");
            _context.AdPlacements.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { websiteId = model.WebsiteId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();
            var placement = await _context.AdPlacements.FindAsync(id);
            if (placement == null) return NotFound();

            return View(placement);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdPlacement model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.AdPlacements.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { websiteId = model.WebsiteId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var placement = await _context.AdPlacements.FindAsync(id);
            if (placement == null) return NotFound();

            return View(placement);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var placement = await _context.AdPlacements.FindAsync(id);
            if (placement == null) return NotFound();

            int websiteId = placement.WebsiteId;

            _context.AdPlacements.Remove(placement);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { websiteId });
        }

        public async Task<IActionResult> Script(int id)
        {
            var placement = await _context.AdPlacements
                .Include(p => p.Website)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (placement == null)
                return NotFound();

            ViewBag.ScriptKey = placement.Website?.ScriptKey;
            return View(placement);
        }
    }
}
