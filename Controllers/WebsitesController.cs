//////using AdSystem.Data;
//////using AdSystem.Models;
//////using Microsoft.AspNetCore.Authorization;
//////using Microsoft.AspNetCore.Identity;
//////using Microsoft.AspNetCore.Mvc;
//////using Microsoft.EntityFrameworkCore;
//////using System.Linq;
//////using System.Threading.Tasks;

//////namespace AdManagementSystem.Controllers
//////{
//////    [Authorize]
//////    public class WebsitesController : Controller
//////    {
//////        private readonly AppDbContext _context;
//////        private readonly UserManager<ApplicationUser> _userManager;

//////        public WebsitesController(AppDbContext context, UserManager<ApplicationUser> userManager)
//////        {
//////            _context = context;
//////            _userManager = userManager;
//////        }

//////        // GET: Websites
//////        public async Task<IActionResult> Index()
//////        {
//////            var user = await _userManager.GetUserAsync(User);
//////            var query = _context.Websites.AsQueryable();

//////            if (User.IsInRole("Admin"))
//////                return View(await query.ToListAsync());

//////            if (User.IsInRole("Publisher"))
//////            {
//////                query = query.Where(w => w.PublisherId == user.Id);
//////                return View(await query.ToListAsync());
//////            }

//////            return Forbid();
//////        }

//////        // GET: Websites/Details/5
//////        public async Task<IActionResult> Details(int? id)
//////        {
//////            if (id == null) return NotFound();

//////            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
//////            if (website == null) return NotFound();

//////            var user = await _userManager.GetUserAsync(User);
//////            if (User.IsInRole("Admin") || website.PublisherId == user.Id)
//////                return View(website);

//////            return Forbid();
//////        }

//////        // GET: Websites/Create
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public IActionResult Create() => View();

//////        // POST: Websites/Create
//////        [HttpPost]
//////        [ValidateAntiForgeryToken]
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public async Task<IActionResult> Create([Bind("Id,Name,Domain,ScriptKey")] Website website)
//////        {
//////            var user = await _userManager.GetUserAsync(User);
//////            if (ModelState.IsValid)
//////            {
//////                website.PublisherId = user.Id;
//////                _context.Add(website);
//////                await _context.SaveChangesAsync();
//////                return RedirectToAction(nameof(Index));
//////            }
//////            return View(website);
//////        }

//////        // GET: Websites/Edit/5
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public async Task<IActionResult> Edit(int? id)
//////        {
//////            if (id == null) return NotFound();

//////            var website = await _context.Websites.FindAsync(id);
//////            if (website == null) return NotFound();

//////            var user = await _userManager.GetUserAsync(User);
//////            if (!User.IsInRole("Admin") && website.PublisherId != user.Id)
//////                return Forbid();

//////            return View(website);
//////        }

//////        // POST: Websites/Edit/5
//////        [HttpPost]
//////        [ValidateAntiForgeryToken]
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Domain,ScriptKey")] Website website)
//////        {
//////            if (id != website.Id) return NotFound();

//////            var existing = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
//////            if (existing == null) return NotFound();

//////            var user = await _userManager.GetUserAsync(User);
//////            if (!User.IsInRole("Admin") && existing.PublisherId != user.Id)
//////                return Forbid();

//////            if (ModelState.IsValid)
//////            {
//////                try
//////                {
//////                    website.PublisherId = existing.PublisherId;
//////                    _context.Update(website);
//////                    await _context.SaveChangesAsync();
//////                }
//////                catch (DbUpdateConcurrencyException)
//////                {
//////                    if (!_context.Websites.Any(e => e.Id == website.Id))
//////                        return NotFound();
//////                    throw;
//////                }
//////                return RedirectToAction(nameof(Index));
//////            }

//////            return View(website);
//////        }

//////        // GET: Websites/Delete/5
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public async Task<IActionResult> Delete(int? id)
//////        {
//////            if (id == null) return NotFound();

//////            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
//////            if (website == null) return NotFound();

//////            var user = await _userManager.GetUserAsync(User);
//////            if (!User.IsInRole("Admin") && website.PublisherId != user.Id)
//////                return Forbid();

//////            return View(website);
//////        }

//////        // POST: Websites/Delete/5
//////        [HttpPost, ActionName("Delete")]
//////        [ValidateAntiForgeryToken]
//////        [Authorize(Roles = "Publisher,Admin")]
//////        public async Task<IActionResult> DeleteConfirmed(int id)
//////        {
//////            var website = await _context.Websites.FindAsync(id);
//////            if (website == null) return NotFound();

//////            var user = await _userManager.GetUserAsync(User);
//////            if (!User.IsInRole("Admin") && website.PublisherId != user.Id)
//////                return Forbid();

//////            _context.Websites.Remove(website);
//////            await _context.SaveChangesAsync();
//////            return RedirectToAction(nameof(Index));
//////        }
//////    }
//////}
////using AdSystem.Data;
////using AdSystem.Models;
////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Identity;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using System.Security.Cryptography;
////using System.Text;
////using System.Threading.Tasks;
////using System.Linq;

////namespace AdManagementSystem.Controllers
////{
////    [Authorize]
////    public class WebsitesController : Controller
////    {
////        private readonly AppDbContext _context;
////        private readonly UserManager<ApplicationUser> _userManager;

////        public WebsitesController(AppDbContext context, UserManager<ApplicationUser> userManager)
////        {
////            _context = context;
////            _userManager = userManager;
////        }

////        private string GenerateScriptKey()
////        {
////            using var rng = RandomNumberGenerator.Create();
////            byte[] bytes = new byte[16];
////            rng.GetBytes(bytes);
////            return Convert.ToHexString(bytes); // example output: B4F3A9C8D21E...
////        }

////        // GET: Websites
////        public async Task<IActionResult> Index()
////        {
////            var user = await _userManager.GetUserAsync(User);

////            var query = _context.Websites.AsQueryable();

////            if (User.IsInRole("Admin"))
////                return View(await query.ToListAsync());

////            if (User.IsInRole("Publisher"))
////            {
////                query = query.Where(w => w.UserId == user.Id);
////                return View(await query.ToListAsync());
////            }

////            return Forbid();
////        }

////        // GET: Websites/Details/5
////        public async Task<IActionResult> Details(int? id)
////        {
////            if (id == null) return NotFound();

////            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
////            if (website == null) return NotFound();

////            var user = await _userManager.GetUserAsync(User);

////            if (User.IsInRole("Admin") || website.UserId == user.Id)
////                return View(website);

////            return Forbid();
////        }

////        // GET: Websites/Create
////        [Authorize(Roles = "Publisher,Admin")]
////        public IActionResult Create() => View();

////        // POST: Websites/Create
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        [Authorize(Roles = "Publisher,Admin")]
////        public async Task<IActionResult> Create([Bind("Name,Url")] Website website)
////        {
////            var user = await _userManager.GetUserAsync(User);

////            if (ModelState.IsValid)
////            {
////                website.UserId = user.Id;
////                website.ScriptKey = GenerateScriptKey();
////                website.IsApproved = User.IsInRole("Admin"); // Admin-created sites auto-approved

////                _context.Add(website);
////                await _context.SaveChangesAsync();
////                return RedirectToAction(nameof(Index));
////            }
////            return View(website);
////        }

////        // GET: Websites/Edit/5
////        [Authorize(Roles = "Publisher,Admin")]
////        public async Task<IActionResult> Edit(int? id)
////        {
////            if (id == null) return NotFound();

////            var website = await _context.Websites.FindAsync(id);
////            if (website == null) return NotFound();

////            var user = await _userManager.GetUserAsync(User);
////            if (!User.IsInRole("Admin") && website.UserId != user.Id)
////                return Forbid();

////            return View(website);
////        }

////        // POST: Websites/Edit/5
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        [Authorize(Roles = "Publisher,Admin")]
////        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Url")] Website website)
////        {
////            if (id != website.Id) return NotFound();

////            var existing = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
////            if (existing == null) return NotFound();

////            var user = await _userManager.GetUserAsync(User);

////            if (!User.IsInRole("Admin") && existing.UserId != user.Id)
////                return Forbid();

////            if (ModelState.IsValid)
////            {
////                website.UserId = existing.UserId;
////                website.ScriptKey = existing.ScriptKey;
////                website.IsApproved = existing.IsApproved; // can only be changed by Admin

////                _context.Update(website);
////                await _context.SaveChangesAsync();
////                return RedirectToAction(nameof(Index));
////            }

////            return View(website);
////        }

////        // Toggle Approve (Admin Only)
////        [Authorize(Roles = "Admin")]
////        public async Task<IActionResult> ToggleApproval(int id)
////        {
////            var site = await _context.Websites.FindAsync(id);
////            if (site == null) return NotFound();

////            site.IsApproved = !site.IsApproved;
////            _context.Update(site);
////            await _context.SaveChangesAsync();

////            return RedirectToAction(nameof(Index));
////        }

////        // GET: Websites/Delete/5
////        [Authorize(Roles = "Publisher,Admin")]
////        public async Task<IActionResult> Delete(int? id)
////        {
////            if (id == null) return NotFound();

////            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
////            if (website == null) return NotFound();

////            var user = await _userManager.GetUserAsync(User);
////            if (!User.IsInRole("Admin") && website.UserId != user.Id)
////                return Forbid();

////            return View(website);
////        }

////        // POST: Websites/Delete/5
////        [HttpPost, ActionName("Delete")]
////        [ValidateAntiForgeryToken]
////        [Authorize(Roles = "Publisher,Admin")]
////        public async Task<IActionResult> DeleteConfirmed(int id)
////        {
////            var website = await _context.Websites.FindAsync(id);
////            if (website == null) return NotFound();

////            var user = await _userManager.GetUserAsync(User);
////            if (!User.IsInRole("Admin") && website.UserId != user.Id)
////                return Forbid();

////            _context.Websites.Remove(website);
////            await _context.SaveChangesAsync();
////            return RedirectToAction(nameof(Index));
////        }
////    }
////}
//using AdSystem.Data;
//using AdSystem.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Cryptography;
//using System.Threading.Tasks;
//using System;
//using System.Linq;

//namespace AdManagementSystem.Controllers
//{
//    [Authorize]
//    public class WebsitesController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public WebsitesController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        private string GenerateScriptKey()
//        {
//            using var rng = RandomNumberGenerator.Create();
//            byte[] bytes = new byte[16];
//            rng.GetBytes(bytes);
//            return Convert.ToHexString(bytes); // Ex: B4F3A9C8D21E4F88A1BF...
//        }

//        // GET: Websites
//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var query = _context.Websites.AsQueryable();

//            if (User.IsInRole("Admin"))
//                return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());

//            if (User.IsInRole("Publisher"))
//            {
//                query = query.Where(w => w.UserId == user.Id);
//                return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
//            }

//            return Forbid();
//        }

//        // GET: Websites/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null) return NotFound();

//            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
//            if (website == null) return NotFound();

//            var user = await _userManager.GetUserAsync(User);

//            if (User.IsInRole("Admin") || website.UserId == user.Id)
//                return View(website);

//            return Forbid();
//        }

//        // GET: Websites/Create
//        [Authorize(Roles = "Publisher,Admin")]
//        public IActionResult Create() => View();

//        // POST: Websites/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> Create([Bind("Name,Url")] Website website)
//        {
//            var user = await _userManager.GetUserAsync(User);

//            if (ModelState.IsValid)
//            {
//                website.UserId = user.Id;
//                website.ScriptKey = GenerateScriptKey();
//                website.CreatedAt = DateTime.UtcNow;
//                website.IsApproved = User.IsInRole("Admin"); // Only auto-approved if created by Admin

//                _context.Add(website);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            return View(website);
//        }

//        // GET: Websites/Edit/5
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            var website = await _context.Websites.FindAsync(id);
//            if (website == null) return NotFound();

//            var user = await _userManager.GetUserAsync(User);
//            if (!User.IsInRole("Admin") && website.UserId != user.Id)
//                return Forbid();

//            return View(website);
//        }

//        // POST: Websites/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Url")] Website website)
//        {
//            if (id != website.Id) return NotFound();

//            var existing = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
//            if (existing == null) return NotFound();

//            var user = await _userManager.GetUserAsync(User);
//            if (!User.IsInRole("Admin") && existing.UserId != user.Id)
//                return Forbid();

//            if (ModelState.IsValid)
//            {
//                // Keep fields that are not editable by Publisher
//                website.UserId = existing.UserId;
//                website.ScriptKey = existing.ScriptKey;
//                website.CreatedAt = existing.CreatedAt;
//                website.IsApproved = existing.IsApproved; // Admin-only via Toggle

//                _context.Update(website);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            return View(website);
//        }

//        // ✅ Admin Approval Toggle
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> ToggleApproval(int id)
//        {
//            var site = await _context.Websites.FindAsync(id);
//            if (site == null) return NotFound();

//            site.IsApproved = !site.IsApproved;
//            _context.Update(site);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }

//        // GET: Websites/Delete/5
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null) return NotFound();

//            var website = await _context.Websites.FirstOrDefaultAsync(m => m.Id == id);
//            if (website == null) return NotFound();

//            var user = await _userManager.GetUserAsync(User);
//            if (!User.IsInRole("Admin") && website.UserId != user.Id)
//                return Forbid();

//            return View(website);
//        }

//        // POST: Websites/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var website = await _context.Websites.FindAsync(id);
//            if (website == null) return NotFound();

//            var user = await _userManager.GetUserAsync(User);
//            if (!User.IsInRole("Admin") && website.UserId != user.Id)
//                return Forbid();

//            _context.Websites.Remove(website);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
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
