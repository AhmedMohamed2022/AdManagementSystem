//using AdSystem.Data;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace AdManagementSystem.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class AdminController : Controller
//    {
//        private readonly AppDbContext _context;
//        public AdminController(AppDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<IActionResult> DashboardAsync()
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
//            return View("~/Views/Admin/Dashboard.cshtml");
//        }
//        public IActionResult ManageUsers()
//        {
//            // later we'll list users & edit roles here
//            return View();
//        }
//        public IActionResult ManageRoles()
//        {
//            return View();
//        }
//    }
//}
