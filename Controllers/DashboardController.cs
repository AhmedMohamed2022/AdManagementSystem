//using AdSystem.Data;
//using AdSystem.Models;
//using AdSystem.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace AdSystem.Controllers
//{
//    [Authorize]
//    public class DashboardController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);

//            if (User.IsInRole("Admin"))
//                return RedirectToAction(nameof(AdminDashboard));

//            if (User.IsInRole("Advertiser"))
//                return RedirectToAction(nameof(AdvertiserDashboard));

//            if (User.IsInRole("Publisher"))
//                return RedirectToAction(nameof(PublisherDashboard));

//            return View("AccessDenied");
//        }

//        // -------------------------------
//        // ADMIN DASHBOARD
//        // -------------------------------
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> AdminDashboard()
//        {
//            var totalUsers = await _context.Users.CountAsync();
//            var totalAds = await _context.Ads.CountAsync();
//            var totalClicks = await _context.Ads.SumAsync(a => (int?)a.Clicks) ?? 0;
//            var totalImpressions = await _context.Ads.SumAsync(a => (int?)a.Impressions) ?? 0;

//            // Optional: group analytics for quick chart previews
//            var adsPerAdvertiser = await _context.Ads
//                .GroupBy(a => a.AdvertiserId)
//                .Select(g => new
//                {
//                    AdvertiserId = g.Key,
//                    Count = g.Count(),
//                    Clicks = g.Sum(a => a.Clicks),
//                    Impressions = g.Sum(a => a.Impressions)
//                })
//                .ToListAsync();

//            ViewBag.TotalUsers = totalUsers;
//            ViewBag.TotalAds = totalAds;
//            ViewBag.TotalClicks = totalClicks;
//            ViewBag.TotalImpressions = totalImpressions;
//            ViewBag.AdsPerAdvertiser = adsPerAdvertiser;
//            return View();
//        }

//        // -------------------------------
//        // ADVERTISER DASHBOARD
//        // -------------------------------
//        [Authorize(Roles = "Advertiser")]
//        public async Task<IActionResult> AdvertiserDashboard()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var myAds = await _context.Ads
//                .Where(a => a.AdvertiserId == user.Id)
//                .ToListAsync();

//            ViewBag.TotalAds = myAds.Count;
//            ViewBag.TotalClicks = myAds.Sum(a => a.Clicks);
//            ViewBag.TotalImpressions = myAds.Sum(a => a.Impressions);

//            return View(myAds);
//        }

//        // -------------------------------
//        // PUBLISHER DASHBOARD
//        // -------------------------------
//        [Authorize(Roles = "Publisher")]
//        public async Task<IActionResult> PublisherDashboard()
//        {
            
//            var user = await _userManager.GetUserAsync(User);

//            // Load websites with ads
//            var myWebsites = await _context.Websites
//                .Where(w => w.UserId == user.Id)
//                .ToListAsync();

//            var vmList = myWebsites.Select(w =>
//            {
//                var ads = w.Ads ?? new List<AdSystem.Models.Ad>();

//                return new PublisherSiteViewModel
//                {
//                    Id = w.Id,
//                    Name = w.Name,
//                    Url = w.Url,
//                    AdsCount = ads.Count,
//                    TotalClicks = ads.Sum(a => a.Clicks),
//                    TotalImpressions = ads.Sum(a => a.Impressions),
//                    CreatedAt = w.CreatedAt // ✅ now read directly
//                };
//            }).ToList();

//            // Totals for summary cards
//            ViewBag.TotalWebsites = vmList.Count;
//            ViewBag.TotalAds = vmList.Sum(x => x.AdsCount);
//            ViewBag.TotalClicks = vmList.Sum(x => x.TotalClicks);
//            ViewBag.TotalImpressions = vmList.Sum(x => x.TotalImpressions);

//            return View(vmList);
//        }
//    }
    
//}
