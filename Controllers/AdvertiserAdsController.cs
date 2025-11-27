using AdManagementSystem.Data;
using AdManagementSystem.Models;
using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AdManagementSystem.Controllers
{
    [Authorize(Roles = "Advertiser")]
    public class AdvertiserAdsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Cloudinary _cloudinary;

        public AdvertiserAdsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            Cloudinary cloudinary)
        {
            _context = context;
            _userManager = userManager;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var ads = await _context.Ads
                .Where(a => a.AdvertiserId == user.Id)
                .Select(a => new AdvertiserAdViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Status = a.Status,
                    Impressions = _context.AdImpressions.Count(i => i.AdId == a.Id),
                    Clicks = _context.AdClicks.Count(c => c.AdId == a.Id),
                    MaxImpressions = a.MaxImpressions,
                    MaxClicks = a.MaxClicks,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                })
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            return View(ads);
        }

        public IActionResult Create()
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ad ad, IFormFile? image)
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(ad);

            var size = await _context.BannerSizes.FindAsync(ad.BannerSizeId);
            if (size == null)
            {
                ModelState.AddModelError("", "Invalid banner size.");
                return View(ad);
            }

            if (image != null)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(image.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP allowed.");
                    return View(ad);
                }

                // Upload to Cloudinary (auto compress + resize)
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    Folder = "ads", // optional
                    Transformation = new Transformation()
                        .Width(size.Width)
                        .Height(size.Height)
                        .Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                ad.ImagePath = uploadResult.SecureUrl.ToString();
                ModelState.Remove("ImagePath");
            }

            ad.AdvertiserId = user.Id;
            ad.Status = AdStatus.Pending;

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();

            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ad ad, IFormFile? image)
        {
            ViewBag.Sizes = _context.BannerSizes.ToList();

            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.Ads
                .Include(a => a.BannerSize)
                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(existing);

            var size = await _context.BannerSizes.FindAsync(ad.BannerSizeId);

            if (image != null)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(image.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP allowed.");
                    return View(existing);
                }

                // Upload new image
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    Folder = "ads",
                    Transformation = new Transformation()
                        .Width(size.Width)
                        .Height(size.Height)
                        .Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // Delete old image from Cloudinary (optional)
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    try
                    {
                        var publicId = GetPublicId(existing.ImagePath);
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                    }
                    catch { }
                }

                existing.ImagePath = uploadResult.SecureUrl.ToString();
                ModelState.Remove("ImagePath");
            }

            existing.Title = ad.Title;
            existing.Description = ad.Description;
            existing.TargetUrl = ad.TargetUrl;
            existing.Status = ad.Status;
            existing.BannerSizeId = ad.BannerSizeId;
            existing.StartDate = ad.StartDate;
            existing.EndDate = ad.EndDate;
            existing.MaxImpressions = ad.MaxImpressions;
            existing.MaxClicks = ad.MaxClicks;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string GetPublicId(string url)
        {
            // Example: https://res.cloudinary.com/xxx/image/upload/v123/ads/filename.jpg
            var parts = url.Split('/');
            var file = parts.Last();
            var folder = parts[parts.Length - 2];
            return $"{folder}/{file.Split('.').First()}";
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads
                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

            if (ad == null) return NotFound();
            return View(ad);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ad = await _context.Ads
                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

            if (ad == null) return NotFound();

            // Delete from Cloudinary
            if (!string.IsNullOrEmpty(ad.ImagePath))
            {
                try
                {
                    var publicId = GetPublicId(ad.ImagePath);
                    await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                }
                catch { }
            }

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
