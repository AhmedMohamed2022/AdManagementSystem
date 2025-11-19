//using Microsoft.AspNetCore.Mvc;
//using AdSystem.Models;
//using AdSystem.Services;

//namespace AdSystem.Controllers
//{
//    public class AdminPricingController : Controller
//    {
//        private readonly IAdPricingService _pricingService;

//        public AdminPricingController(IAdPricingService pricingService)
//        {
//            _pricingService = pricingService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var data = await _pricingService.GetAllAsync();
//            return View(data);
//        }

//        public IActionResult Create()
//        {
//            return View(new AdPricingRule());
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(AdPricingRule rule)
//        {
//            if (ModelState.IsValid)
//            {
//                await _pricingService.CreateAsync(rule);
//                return RedirectToAction(nameof(Index));
//            }
//            return View(rule);
//        }

//        public async Task<IActionResult> Edit(int id)
//        {
//            var rule = await _pricingService.GetByIdAsync(id);
//            if (rule == null) return NotFound();
//            return View(rule);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(AdPricingRule rule)
//        {
//            if (ModelState.IsValid)
//            {
//                await _pricingService.UpdateAsync(rule);
//                return RedirectToAction(nameof(Index));
//            }
//            return View(rule);
//        }

//        public async Task<IActionResult> Delete(int id)
//        {
//            var rule = await _pricingService.GetByIdAsync(id);
//            if (rule == null) return NotFound();
//            return View(rule);
//        }

//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            await _pricingService.DeleteAsync(id);
//            return RedirectToAction(nameof(Index));
//        }

//        public async Task<IActionResult> Details(int id)
//        {
//            var rule = await _pricingService.GetByIdAsync(id);
//            if (rule == null) return NotFound();
//            return View(rule);
//        }
//    }
//}

using AdSystem.Models;
using AdSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPricingController : Controller
    {
        private readonly IAdPricingService _pricingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminPricingController(IAdPricingService pricingService, UserManager<ApplicationUser> userManager)
        {
            _pricingService = pricingService;
            _userManager = userManager;
        }

        // GET: /AdminPricing
        public async Task<IActionResult> Index()
        {
            var rules = await _pricingService.GetAllAsync();
            return View(rules);
        }

        // GET: /AdminPricing/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Advertisers = await _userManager.GetUsersInRoleAsync("Advertiser");
            // Add countries list
            ViewBag.Countries = GetCountriesList();

            // Add cities (can be populated via AJAX based on country selection)
            ViewBag.Cities = new List<string>(); // Empty initially

            return View(new AdPricingRule { CPM = 1, CPC = 0.05M });
        }

        // POST: /AdminPricing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdPricingRule rule)
        {
            if (!ModelState.IsValid)
            {
                return View(rule);
            }

            rule.CreatedAt = DateTime.UtcNow;
            await _pricingService.CreateAsync(rule);
            return RedirectToAction(nameof(Index));
        }
        // Helper method to get countries
        private List<SelectListItem> GetCountriesList()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "-- اختر دولة --" },
        new SelectListItem { Value = "Egypt", Text = "مصر (Egypt)" },
        new SelectListItem { Value = "Saudi Arabia", Text = "السعودية (Saudi Arabia)" },
        new SelectListItem { Value = "United Arab Emirates", Text = "الإمارات (UAE)" },
        new SelectListItem { Value = "Kuwait", Text = "الكويت (Kuwait)" },
        new SelectListItem { Value = "Qatar", Text = "قطر (Qatar)" },
        new SelectListItem { Value = "Bahrain", Text = "البحرين (Bahrain)" },
        new SelectListItem { Value = "Oman", Text = "عمان (Oman)" },
        new SelectListItem { Value = "Jordan", Text = "الأردن (Jordan)" },
        new SelectListItem { Value = "Lebanon", Text = "لبنان (Lebanon)" },
        new SelectListItem { Value = "Palestine", Text = "فلسطين (Palestine)" },
        new SelectListItem { Value = "Iraq", Text = "العراق (Iraq)" },
        new SelectListItem { Value = "Syria", Text = "سوريا (Syria)" },
        new SelectListItem { Value = "Yemen", Text = "اليمن (Yemen)" },
        new SelectListItem { Value = "Libya", Text = "ليبيا (Libya)" },
        new SelectListItem { Value = "Tunisia", Text = "تونس (Tunisia)" },
        new SelectListItem { Value = "Algeria", Text = "الجزائر (Algeria)" },
        new SelectListItem { Value = "Morocco", Text = "المغرب (Morocco)" },
        new SelectListItem { Value = "Sudan", Text = "السودان (Sudan)" },
        new SelectListItem { Value = "Turkey", Text = "تركيا (Turkey)" },
        new SelectListItem { Value = "United States", Text = "الولايات المتحدة (USA)" },
        new SelectListItem { Value = "United Kingdom", Text = "المملكة المتحدة (UK)" },
        new SelectListItem { Value = "Germany", Text = "ألمانيا (Germany)" },
        new SelectListItem { Value = "France", Text = "فرنسا (France)" },
        new SelectListItem { Value = "Canada", Text = "كندا (Canada)" },
        new SelectListItem { Value = "Australia", Text = "أستراليا (Australia)" },
        // Add more countries as needed
    };
        }

        // API endpoint to get cities by country (called via AJAX)
        [HttpGet]
        public JsonResult GetCitiesByCountry(string country)
        {
            var cities = GetCitiesForCountry(country);
            return Json(cities);
        }

        private List<SelectListItem> GetCitiesForCountry(string country)
        {
            // This is a sample - you can expand this or use a database
            var citiesDict = new Dictionary<string, List<string>>
            {
                ["Egypt"] = new List<string> { "Cairo", "Alexandria", "Giza", "Shubra El-Kheima", "Port Said", "Suez", "Luxor", "Mansoura", "Tanta", "Aswan" },
                ["Saudi Arabia"] = new List<string> { "Riyadh", "Jeddah", "Mecca", "Medina", "Dammam", "Khobar", "Dhahran", "Tabuk", "Buraidah", "Khamis Mushait" },
                ["United Arab Emirates"] = new List<string> { "Dubai", "Abu Dhabi", "Sharjah", "Ajman", "Ras Al Khaimah", "Fujairah", "Umm Al Quwain", "Al Ain" },
                ["Kuwait"] = new List<string> { "Kuwait City", "Hawalli", "Salmiya", "Farwaniya", "Fahaheel", "Jahra", "Ahmadi" },
                ["Qatar"] = new List<string> { "Doha", "Al Wakrah", "Al Rayyan", "Umm Salal", "Al Khor", "Dukhan" },
                ["Jordan"] = new List<string> { "Amman", "Zarqa", "Irbid", "Aqaba", "Madaba", "Jerash", "Petra", "Karak" },
                ["United States"] = new List<string> { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" },
                ["United Kingdom"] = new List<string> { "London", "Birmingham", "Manchester", "Leeds", "Liverpool", "Newcastle", "Sheffield", "Bristol", "Glasgow", "Edinburgh" },
                // Add more countries and their cities
            };

            if (citiesDict.ContainsKey(country))
            {
                return citiesDict[country].Select(city => new SelectListItem
                {
                    Value = city,
                    Text = city
                }).ToList();
            }

            return new List<SelectListItem>();
        }
        // GET: /AdminPricing/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var rule = await _pricingService.GetByIdAsync(id);
            if (rule == null) return NotFound();

            ViewBag.Advertisers = await _userManager.GetUsersInRoleAsync("Advertiser");
            return View(rule);
        }

        // POST: /AdminPricing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdPricingRule rule)
        {
            if (!ModelState.IsValid)
            {
                return View(rule);
            }

            await _pricingService.UpdateAsync(rule);
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminPricing/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var rule = await _pricingService.GetByIdAsync(id);
            if (rule == null) return NotFound();
            return View(rule);
        }

        // POST: /AdminPricing/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _pricingService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ✅ Test pricing engine
        // GET: /AdminPricing/Test?advertiserId=...&country=...&city=...
        public async Task<IActionResult> Test(string? advertiserId, string? country, string? city)
        {
            var rule = await _pricingService.GetEffectivePricingAsync(advertiserId, country, city);
            return Json(new
            {
                appliedRuleId = rule.Id,
                ruleType = rule.RuleType,
                ruleCountry = rule.Country,
                ruleCity = rule.City,
                ruleAdvertiser = rule.AdvertiserId,
                CPM = rule.CPM,
                CPC = rule.CPC
            });
        }
    }
}
