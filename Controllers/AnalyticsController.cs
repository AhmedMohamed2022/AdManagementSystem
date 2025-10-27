//using AdSystem.Data;
//using AdSystem.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace AdSystem.Controllers
//{
//    [Authorize]
//    [ApiController]
//    [Route("Analytics")]
//    public class AnalyticsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public AnalyticsController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // ---------------------------------------------
//        // GET: /Analytics/GetAdvertiserAdStats
//        // For advertisers: shows their ads’ clicks/impressions grouped by date
//        // ---------------------------------------------
//        //[HttpGet("GetAdvertiserAdStats")]
//        //[Authorize(Roles = "Advertiser,Admin")]
//        //public async Task<IActionResult> GetAdvertiserAdStats()
//        //{
//        //    var user = await _userManager.GetUserAsync(User);

//        //    var query = _context.Ads.AsQueryable();

//        //    // If advertiser, show only their ads
//        //    if (User.IsInRole("Advertiser"))
//        //        query = query.Where(a => a.AdvertiserId == user.Id);

//        //    // Example analytics grouping by CreatedAt (or any relevant date)
//        //    var data = await query
//        //        .GroupBy(a => a.CreatedAt.Date)
//        //        .Select(g => new
//        //        {
//        //            Date = g.Key.ToString("yyyy-MM-dd"),
//        //            TotalClicks = g.Sum(a => a.Clicks),
//        //            TotalImpressions = g.Sum(a => a.Impressions)
//        //        })
//        //        .OrderBy(x => x.Date)
//        //        .ToListAsync();

//        //    return Ok(data);
//        //}
//        // GET: /Analytics/GetAdvertiserAdStats?from=2025-10-01&to=2025-10-22
//        [HttpGet("GetAdvertiserAdStats")]
//        [Authorize(Roles = "Advertiser,Admin")]
//        public async Task<IActionResult> GetAdvertiserAdStats([FromQuery] string? from, [FromQuery] string? to)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var query = _context.Ads.AsQueryable();

//            if (User.IsInRole("Advertiser"))
//                query = query.Where(a => a.AdvertiserId == user.Id);

//            // Apply optional date filters (CreatedAt)
//            DateTime? fromDt = null;
//            DateTime? toDt = null;
//            if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out var f)) fromDt = f.Date;
//            if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out var t)) toDt = t.Date.AddDays(1).AddTicks(-1);

//            if (fromDt.HasValue) query = query.Where(a => a.CreatedAt >= fromDt.Value);
//            if (toDt.HasValue) query = query.Where(a => a.CreatedAt <= toDt.Value);

//            // Group by the Date (Date portion) — EF can translate CreatedAt.Date
//            var raw = await query
//                .GroupBy(a => a.CreatedAt.Date)
//                .Select(g => new
//                {
//                    Date = g.Key, // DateTime (date part)
//                    TotalClicks = g.Sum(a => a.Clicks),
//                    TotalImpressions = g.Sum(a => a.Impressions)
//                })
//                .OrderBy(x => x.Date)
//                .ToListAsync();

//            // Format date to yyyy-MM-dd in memory (safe)
//            var result = raw.Select(r => new
//            {
//                date = r.Date.ToString("yyyy-MM-dd"),
//                totalClicks = r.TotalClicks,
//                totalImpressions = r.TotalImpressions
//            }).ToList();

//            return Ok(result);
//        }


//        // ---------------------------------------------
//        // GET: /Analytics/GetPublisherSiteStats
//        // For publishers: shows their websites’ clicks/impressions grouped by site
//        // ---------------------------------------------
//        [HttpGet("GetPublisherSiteStats")]
//        [Authorize(Roles = "Publisher,Admin")]
//        public async Task<IActionResult> GetPublisherSiteStats()
//        {
//            var user = await _userManager.GetUserAsync(User);

//            var query = _context.Websites

//                .AsQueryable();

//            // If publisher, show only their websites
//            if (User.IsInRole("Publisher"))
//                query = query.Where(w => w.UserId == user.Id);

//            var data = await query
//                .Select(w => new
//                {
//                    WebsiteName = w.Name,
//                    Domain = w.Url,
//                    TotalClicks = w.Ads.Sum(a => a.Clicks),
//                    TotalImpressions = w.Ads.Sum(a => a.Impressions)
//                })
//                .OrderByDescending(x => x.TotalImpressions)
//                .ToListAsync();

//            return Ok(data);
//        }
//    }
//}
