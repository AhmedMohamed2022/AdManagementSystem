//using AdManagementSystem.Data;
//using AdManagementSystem.Models;
//using AdManagementSystem.Models.Enums;
//using AdSystem.Data;
//using AdSystem.Models;
//using AdSystem.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;
//using SixLabors.ImageSharp;

//namespace AdManagementSystem.Controllers
//{
//    [Authorize(Roles = "Advertiser")]
//    public class AdvertiserAdsController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public AdvertiserAdsController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        //// ==========================
//        //// 🔹 GET: /AdvertiserAds
//        //// ==========================
//        //public async Task<IActionResult> Index()
//        //{
//        //    var user = await _userManager.GetUserAsync(User);
//        //    var ads = _context.Ads
//        //        .Where(a => a.AdvertiserId == user.Id)
//        //        .OrderByDescending(a => a.Id)
//        //        .ToList();

//        //    return View(ads);
//        //}
//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ads = await _context.Ads
//                .Where(a => a.AdvertiserId == user.Id)
//                .Select(a => new AdvertiserAdViewModel
//                {
//                    Id = a.Id,
//                    Title = a.Title,
//                    Status = a.Status,
//                    Impressions = _context.AdImpressions.Count(i => i.AdId == a.Id),
//                    Clicks = _context.AdClicks.Count(c => c.AdId == a.Id)
//                })
//                .OrderByDescending(a => a.Id)
//                .ToListAsync();

//            return View(ads);
//        }

//        // ==========================
//        // 🔹 GET: /AdvertiserAds/Create
//        // ==========================
//        public IActionResult Create() => View();

//        // ==========================
//        // 🔹 POST: /AdvertiserAds/Create
//        // ==========================
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> Create(Ad ad)
//        //{
//        //    var user = await _userManager.GetUserAsync(User);

//        //    if (!ModelState.IsValid)
//        //        return View(ad);

//        //    ad.AdvertiserId = user.Id;
//        //    ad.Status = AdStatus.Pending; // Admin must approve (optional future rule)
//        //    _context.Ads.Add(ad);
//        //    await _context.SaveChangesAsync();

//        //    return RedirectToAction(nameof(Index));
//        //}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Ad ad, IFormFile? image)
//        {
//            var user = await _userManager.GetUserAsync(User);

//            if (!ModelState.IsValid)
//                return View(ad);

//            // ✅ Image validation
//            if (image != null)
//            {
//                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
//                var ext = Path.GetExtension(image.FileName).ToLower();

//                if (!allowed.Contains(ext))
//                {
//                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP images allowed.");
//                    return View(ad);
//                }

//                if (image.Length > 150 * 1024)
//                {
//                    ModelState.AddModelError("", "Image must be under 150KB.");
//                    return View(ad);
//                }

//                using var img = Image.Load(image.OpenReadStream());
//                if (img.Width > 800 || img.Height > 800)
//                {
//                    ModelState.AddModelError("", "Maximum image size is 800x800px.");
//                    return View(ad);
//                }

//                var fileName = Guid.NewGuid() + ext;
//                var savePath = Path.Combine("wwwroot", "ads", fileName);
//                using var stream = new FileStream(savePath, FileMode.Create);
//                await image.CopyToAsync(stream);

//                ad.ImageUrl = "/ads/" + fileName;
//            }

//            ad.AdvertiserId = user.Id;
//            ad.Status = AdStatus.Pending;
//            _context.Ads.Add(ad);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }


//        // ==========================
//        // 🔹 GET: /AdvertiserAds/Edit/5
//        // ==========================
//        public async Task<IActionResult> Edit(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//            if (ad == null) return NotFound();

//            return View(ad);
//        }

//        // ==========================
//        // 🔹 POST: /AdvertiserAds/Edit/5
//        // ==========================
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> Edit(int id, Ad ad)
//        //{
//        //    var user = await _userManager.GetUserAsync(User);
//        //    var existing = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//        //    if (existing == null) return NotFound();

//        //    if (!ModelState.IsValid)
//        //        return View(ad);

//        //    existing.Title = ad.Title;
//        //    existing.Description = ad.Description;
//        //    existing.ImageUrl = ad.ImageUrl;
//        //    existing.TargetUrl = ad.TargetUrl;
//        //    existing.Status = ad.Status;

//        //    await _context.SaveChangesAsync();
//        //    return RedirectToAction(nameof(Index));
//        //}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Ad ad, IFormFile? image)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var existing = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//            if (existing == null) return NotFound();

//            if (!ModelState.IsValid)
//                return View(ad);

//            if (image != null)
//            {
//                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
//                var ext = Path.GetExtension(image.FileName).ToLower();

//                if (!allowed.Contains(ext))
//                {
//                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP images allowed.");
//                    return View(ad);
//                }

//                if (image.Length > 150 * 1024)
//                {
//                    ModelState.AddModelError("", "Image must be under 150KB.");
//                    return View(ad);
//                }

//                using var img = Image.Load(image.OpenReadStream());

//                if (img.Width > 800 || img.Height > 800)
//                {
//                    ModelState.AddModelError("", "Maximum image size is 800x800px.");
//                    return View(ad);
//                }

//                var fileName = Guid.NewGuid() + ext;
//                var savePath = Path.Combine("wwwroot", "ads", fileName);
//                using var stream = new FileStream(savePath, FileMode.Create);
//                await image.CopyToAsync(stream);

//                existing.ImageUrl = "/ads/" + fileName;
//            }

//            existing.Title = ad.Title;
//            existing.Description = ad.Description;
//            existing.TargetUrl = ad.TargetUrl;
//            existing.Status = ad.Status;

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }


//        // ==========================
//        // 🔹 GET: /AdvertiserAds/Delete/5
//        // ==========================

//        public async Task<IActionResult> Delete(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//            if (ad == null) return NotFound();

//            return View(ad);
//        }

//        // ==========================
//        // 🔹 POST: /AdvertiserAds/DeleteConfirmed/5
//        // ==========================
//        [HttpPost, ActionName("DeleteConfirmed")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//            if (ad == null) return NotFound();

//            _context.Ads.Remove(ad);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//    }
//}

//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Create(Ad ad, IFormFile? image)
//{
//    var user = await _userManager.GetUserAsync(User);

//    if (!ModelState.IsValid)
//        return View(ad);

//    if (image != null)
//    {
//        var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
//        var ext = Path.GetExtension(image.FileName).ToLower();

//        if (!allowed.Contains(ext))
//        {
//            ModelState.AddModelError("", "Only PNG, JPG, or WEBP images allowed.");
//            return View(ad);
//        }

//        if (image.Length > 150 * 1024)
//        {
//            ModelState.AddModelError("", "Image must be under 150KB.");
//            return View(ad);
//        }

//        using var img = await Image.LoadAsync(image.OpenReadStream());
//        if (img.Width > 800 || img.Height > 800)
//        {
//            ModelState.AddModelError("", "Maximum image size is 800x800px.");
//            return View(ad);
//        }

//        var fileName = Guid.NewGuid() + ext;
//        var savePath = Path.Combine("wwwroot", "ads", fileName);

//        using var fs = System.IO.File.Create(savePath);
//        await image.CopyToAsync(fs);

//        ad.ImagePath = "/ads/" + fileName;
//    }

//    ad.AdvertiserId = user.Id;
//    ad.Status = AdStatus.Pending;

//    _context.Ads.Add(ad);
//    await _context.SaveChangesAsync();

//    return RedirectToAction(nameof(Index));
//}

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

//namespace AdManagementSystem.Controllers
//{
//    [Authorize(Roles = "Advertiser")]
//    public class AdvertiserAdsController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public AdvertiserAdsController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);

//            var ads = await _context.Ads
//                .Where(a => a.AdvertiserId == user.Id)
//                .Select(a => new AdvertiserAdViewModel
//                {
//                    Id = a.Id,
//                    Title = a.Title,
//                    Status = a.Status,
//                    Impressions = _context.AdImpressions.Count(i => i.AdId == a.Id),
//                    Clicks = _context.AdClicks.Count(c => c.AdId == a.Id),
//                    MaxImpressions = a.MaxImpressions,
//                    MaxClicks = a.MaxClicks,
//                    StartDate = a.StartDate,
//                    EndDate = a.EndDate
//                })
//                .OrderByDescending(a => a.Id)
//                .ToListAsync();

//            return View(ads);
//        }

//        public IActionResult Create()
//        {
//            ViewBag.Sizes = _context.BannerSizes.ToList();

//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Ad ad, IFormFile? image)
//        {
//            ViewBag.Sizes = _context.BannerSizes.ToList();
//            var user = await _userManager.GetUserAsync(User);

//            if (!ModelState.IsValid)
//                return View(ad);

//            var size = await _context.BannerSizes.FindAsync(ad.BannerSizeId);
//            if (size == null)
//            {
//                ModelState.AddModelError("", "Invalid banner size.");
//                return View(ad);
//            }

//            if (image != null)
//            {
//                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
//                var ext = Path.GetExtension(image.FileName).ToLower();
//                if (!allowed.Contains(ext))
//                {
//                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP allowed.");
//                    return View(ad);
//                }

//                using var img = await Image.LoadAsync(image.OpenReadStream());

//                if (img.Width < size.Width || img.Height < size.Height)
//                {
//                    ModelState.AddModelError("", $"Image must be at least {size.Width}x{size.Height}px.");
//                    return View(ad);
//                }

//                img.Mutate(x => x.Resize(size.Width, size.Height));

//                var fileName = Guid.NewGuid() + ext;
//                var folderPath = Path.Combine("wwwroot", "ads");

//                // ✅ Ensure folder exists
//                if (!Directory.Exists(folderPath))
//                    Directory.CreateDirectory(folderPath);

//                var savePath = Path.Combine(folderPath, fileName);

//                await img.SaveAsync(savePath);

//                ad.ImagePath = "/ads/" + fileName;
//                ModelState.Remove("ImagePath"); // ensure ModelState becomes valid
//            }

//            ad.AdvertiserId = user.Id;
//            ad.Status = AdStatus.Pending;

//            _context.Ads.Add(ad);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }


//        public async Task<IActionResult> Edit(int id)
//        {
//            ViewBag.Sizes = _context.BannerSizes.ToList();

//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);
//            if (ad == null) return NotFound();

//            return View(ad);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Ad ad, IFormFile? image)
//        {
//            ViewBag.Sizes = _context.BannerSizes.ToList();

//            var user = await _userManager.GetUserAsync(User);
//            var existing = await _context.Ads
//                .Include(a => a.BannerSize)
//                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

//            if (existing == null)
//                return NotFound();

//            if (!ModelState.IsValid)
//                return View(existing);

//            var size = await _context.BannerSizes.FindAsync(ad.BannerSizeId);
//            if (size == null)
//            {
//                ModelState.AddModelError("", "Invalid banner size.");
//                return View(existing);
//            }

//            // ✅ If a new image is uploaded → delete old safely
//            if (image != null)
//            {
//                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
//                var ext = Path.GetExtension(image.FileName).ToLower();

//                if (!allowed.Contains(ext))
//                {
//                    ModelState.AddModelError("", "Only PNG, JPG, or WEBP allowed.");
//                    return View(existing);
//                }

//                using var img = await Image.LoadAsync(image.OpenReadStream());

//                // ✅ Validate against selected banner size
//                if (img.Width < size.Width || img.Height < size.Height)
//                {
//                    ModelState.AddModelError("", $"Image must be at least {size.Width}x{size.Height}px.");
//                    return View(existing);
//                }

//                img.Mutate(x => x.Resize(size.Width, size.Height));

//                var fileName = Guid.NewGuid() + ext;
//                var folderPath = Path.Combine("wwwroot", "ads");

//                if (!Directory.Exists(folderPath))
//                    Directory.CreateDirectory(folderPath);

//                var savePath = Path.Combine(folderPath, fileName);
//                await img.SaveAsync(savePath);

//                // ✅ delete old image safely
//                if (!string.IsNullOrEmpty(existing.ImagePath))
//                {
//                    var oldFile = Path.Combine("wwwroot", existing.ImagePath.TrimStart('/'));
//                    if (System.IO.File.Exists(oldFile))
//                        System.IO.File.Delete(oldFile);
//                }

//                existing.ImagePath = "/ads/" + fileName;
//                ModelState.Remove("ImagePath"); // avoid invalid ModelState
//            }

//            // ✅ Update normal fields
//            existing.Title = ad.Title;
//            existing.Description = ad.Description;
//            existing.TargetUrl = ad.TargetUrl;
//            existing.Status = ad.Status;
//            existing.BannerSizeId = ad.BannerSizeId;
//            existing.StartDate = ad.StartDate;
//            existing.EndDate = ad.EndDate;
//            existing.MaxImpressions = ad.MaxImpressions;
//            existing.MaxClicks = ad.MaxClicks;

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        public async Task<IActionResult> Delete(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads
//                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

//            if (ad == null) return NotFound();
//            return View(ad);
//        }

//        [HttpPost, ActionName("DeleteConfirmed")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var ad = await _context.Ads
//                .FirstOrDefaultAsync(a => a.Id == id && a.AdvertiserId == user.Id);

//            if (ad == null) return NotFound();

//            // ✅ Remove image file safely
//            if (!string.IsNullOrEmpty(ad.ImagePath))
//            {
//                var file = Path.Combine("wwwroot", ad.ImagePath.TrimStart('/'));
//                if (System.IO.File.Exists(file))
//                    System.IO.File.Delete(file);
//            }

//            _context.Ads.Remove(ad);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//    }
//}
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
