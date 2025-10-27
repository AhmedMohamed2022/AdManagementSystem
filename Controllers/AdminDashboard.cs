using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

    public AdminDashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager; // Initialize UserManager
    }

    public IActionResult Index()
    {
        var advertisers = _userManager.GetUsersInRoleAsync("Advertiser").Result;
        var publishers = _userManager.GetUsersInRoleAsync("Publisher").Result;

        var model = new AdminDashboardViewModel
        {
            TotalAdvertisers = advertisers.Count, // Use UserManager to get count
            TotalPublishers = publishers.Count, // Use UserManager to get count
            TotalAds = _context.Ads.Count(),
            TotalWebsites = _context.Websites.Count(),
            PendingWebsites = _context.Websites.Count(w => !w.IsApproved),
            ActiveAds = _context.Ads.Count(a => a.Status == AdStatus.Approved)
        };

        return View(model);
    }
}
