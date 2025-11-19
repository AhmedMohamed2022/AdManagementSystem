//using AdSystem.Data;
//using AdSystem.Models;
//using AdSystem.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//[Authorize(Roles = "Publisher")]
//public class PublisherDashboardController : Controller
//{
//    private readonly AppDbContext _context;
//    private readonly UserManager<ApplicationUser> _userManager;

//    public PublisherDashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
//    {
//        _context = context;
//        _userManager = userManager;
//    }

//    public async Task<IActionResult> Index()
//    {
//        var user = await _userManager.GetUserAsync(User);

//        var websites = _context.Websites.Where(w => w.OwnerId == user.Id);
//        var model = new PublisherDashboardViewModel
//        {
//            TotalWebsites = websites.Count(),
//            ApprovedSites = websites.Count(w => w.IsApproved),
//            PendingSites = websites.Count(w => !w.IsApproved),
//            TotalImpressions = _context.AdImpressions.Count(i => i.Website.OwnerId == user.Id),
//            TotalClicks = _context.AdClicks.Count(c => c.Website.OwnerId == user.Id)
//        };

//        return View(model);
//    }
//}
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Publisher")]
public class PublisherDashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PublisherDashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var websites = await _context.Websites
            .Where(w => w.OwnerId == user.Id)
            .ToListAsync();

        var model = new PublisherDashboardViewModel
        {
            TotalWebsites = websites.Count,
            ApprovedSites = websites.Count(w => w.IsApproved),
            PendingSites = websites.Count(w => !w.IsApproved)
        };

        decimal totalEarnings = 0;

        foreach (var site in websites)
        {
            var impressions = await _context.AdImpressions
                .Where(i => i.WebsiteId == site.Id)
                .ToListAsync();

            var clicks = await _context.AdClicks
                .Where(c => c.WebsiteId == site.Id)
                .ToListAsync();

            var impCount = impressions.Count;
            var clkCount = clicks.Count;

            var earned = impressions.Sum(i => i.EarnedAmount) +
                         clicks.Sum(c => c.EarnedAmount);

            totalEarnings += earned;

            model.Earnings.Add(new PublisherWebsiteEarning
            {
                WebsiteName = site.Name,
                Domain = site.Domain,
                IsApproved = site.IsApproved,
                Impressions = impCount,
                Clicks = clkCount,
                TotalEarned = Math.Round(earned, 2)
            });

            model.TotalImpressions += impCount;
            model.TotalClicks += clkCount;
        }

        model.TotalEarnings = Math.Round(totalEarnings, 2);

        return View(model);
    }
}
