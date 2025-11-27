using AdManagementSystem.Data;
using AdManagementSystem.Models;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Publisher,Admin")]
    public class WebsitesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WebsitesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================
        // 🔹 GET: /Websites
        // ==========================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var websites = _context.Websites
                .Where(w => w.OwnerId == user.Id)
                .ToList();

            return View(websites);
        }

        // ==========================
        // 🔹 GET: /Websites/Create
        // ==========================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================
        // 🔹 POST: /Websites/Create
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Website website)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
                return View(website);

            website.OwnerId = user.Id;
            website.IsApproved = false;

            _context.Websites.Add(website);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // 🔹 GET: /Websites/Script/5
        // ==========================
        public async Task<IActionResult> Script(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null || website.OwnerId != _userManager.GetUserId(User))
                return NotFound();

            // Generate snippet for publisher
            string snippet = $@"<script src=""https://{Request.Host}/js/ad.js?key={website.ScriptKey}"" ></script>
<div id=""ad-container""></div>";

            ViewBag.Snippet = snippet;
            return View(website);
        }
        // ==========================
        // 🔹 GET: /Websites/Edit/5
        // ==========================
        public async Task<IActionResult> Edit(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null || website.OwnerId != _userManager.GetUserId(User))
                return NotFound();

            return View(website);
        }

        // ==========================
        // 🔹 POST: /Websites/Edit/5
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Website website)
        {
            if (id != website.Id) return NotFound();
            var userId = _userManager.GetUserId(User);

            var existing = await _context.Websites.FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == userId);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return View(website);

            existing.Domain = website.Domain;
            existing.Name = website.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // 🔹 GET: /Websites/Delete/5
        // ==========================
        public async Task<IActionResult> Delete(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null || website.OwnerId != _userManager.GetUserId(User))
                return NotFound();

            return View(website);
        }

        // ==========================
        // 🔹 POST: /Websites/DeleteConfirmed/5
        // ==========================
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null || website.OwnerId != _userManager.GetUserId(User))
                return NotFound();

            _context.Websites.Remove(website);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
