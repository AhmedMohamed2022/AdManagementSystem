using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        var websites = _context.Websites.Where(w => w.OwnerId == user.Id);
        var model = new PublisherDashboardViewModel
        {
            TotalWebsites = websites.Count(),
            ApprovedSites = websites.Count(w => w.IsApproved),
            PendingSites = websites.Count(w => !w.IsApproved),
            TotalImpressions = _context.AdImpressions.Count(i => i.Website.OwnerId == user.Id),
            TotalClicks = _context.AdClicks.Count(c => c.Website.OwnerId == user.Id)
        };

        return View(model);
    }
}
