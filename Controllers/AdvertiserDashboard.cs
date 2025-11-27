using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.Services;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Advertiser")]
public class AdvertiserDashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdPricingService _pricingService;

    public AdvertiserDashboardController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IAdPricingService pricingService)
    {
        _context = context;
        _userManager = userManager;
        _pricingService = pricingService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var ads = _context.Ads
            .Where(a => a.AdvertiserId == user.Id)
            .ToList();

        var impressions = _context.AdImpressions
            .Where(i => i.Ad.AdvertiserId == user.Id)
            .Count();

        var clicks = _context.AdClicks
            .Where(c => c.Ad.AdvertiserId == user.Id)
            .Count();

        var pricing = await _pricingService.GetEffectivePricingAsync(
            advertiserId: user.Id,
            country: null,
            city: null
        );

        decimal totalCost = 0;
        var billingRows = new List<AdBillingRow>();

        foreach (var ad in ads)
        {
            var adImpressions = _context.AdImpressions.Count(i => i.AdId == ad.Id);
            var adClicks = _context.AdClicks.Count(c => c.AdId == ad.Id);

            decimal impressionCost = (pricing.CPM / 1000M) * adImpressions;
            decimal clickCost = pricing.CPC * adClicks;

            decimal adTotal = impressionCost + clickCost;
            totalCost += adTotal;

            billingRows.Add(new AdBillingRow
            {
                AdId = ad.Id,
                AdName = ad.Title,
                Impressions = adImpressions,
                Clicks = adClicks,
                CPM = pricing.CPM,
                CPC = pricing.CPC,
                TotalCost = Math.Round(adTotal, 2)
            });
        }

        var model = new AdvertiserDashboardViewModel
        {
            TotalAds = ads.Count,
            ApprovedAds = ads.Count(a => a.Status == AdStatus.Approved),
            PendingAds = ads.Count(a => a.Status == AdStatus.Pending),
            TotalClicks = clicks,
            TotalImpressions = impressions,

            AppliedCPM = pricing.CPM,
            AppliedCPC = pricing.CPC,
            PricingRuleType = pricing.RuleType.ToString(),
            PricingCountry = pricing.Country ?? "-",
            PricingCity = pricing.City ?? "-",
            EstimatedSpend = Math.Round(totalCost, 2),

            BillingRows = billingRows
        };

        return View(model);
    }
}
