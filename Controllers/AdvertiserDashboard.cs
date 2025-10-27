using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Advertiser")]
public class AdvertiserDashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdvertiserDashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var ads = _context.Ads.Where(a => a.AdvertiserId == user.Id);

        var model = new AdvertiserDashboardViewModel
        {
            TotalAds = ads.Count(),
            ApprovedAds = ads.Count(a => a.Status == AdStatus.Approved),
            PendingAds = ads.Count(a => a.Status == AdStatus.Pending),
            TotalClicks = _context.AdClicks.Count(c => c.Ad.AdvertiserId == user.Id),
            TotalImpressions = _context.AdImpressions.Count(i => i.Ad.AdvertiserId == user.Id)
        };

        return View(model);
    }
}
