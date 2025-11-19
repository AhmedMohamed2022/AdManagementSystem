using AdManagementSystem.Services;
using AdSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Controllers
{
    /// <summary>
    /// API controller that handles ad serving and click tracking.
    /// </summary>
    [Route("ad")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private readonly IAdService _adService;
        private readonly AppDbContext _context;


        public AdController(IAdService adService,AppDbContext context)
        {
            _adService = adService;
            _context = context;
        }

        // ==========================
        // 🔹 GET: /ad/get?host=example.com
        // ==========================
        [HttpGet("get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAd([FromQuery] string key)
        {
            // Resolve website from key
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            string? hostDomain = website?.Domain;

            var ad = await _adService.GetRandomEligibleAdAsync(hostDomain);
            if (ad == null)
                return NotFound(new { message = "No eligible ads available." });

            return Ok(new
            {
                ad.Id,
                ad.Title,
                ad.ImagePath,
                ad.TargetUrl,
                ad.Description
            });
        }


        //// ==========================
        //// 🔹 GET: /ad/click?adId=5&host=example.com
        //// ==========================
        //[HttpGet("click")]
        //[AllowAnonymous]
        //public async Task<IActionResult> RegisterClick([FromQuery] int adId, [FromQuery] string key)
        //{
        //    var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key);
        //    string? hostDomain = website?.Domain;

        //    string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        //    string? ua = Request.Headers["User-Agent"].ToString();

        //    await _adService.RecordClickAsync(adId, hostDomain, ip, ua);

        //    var redirect = await _adService.GetAdTargetUrlAsync(adId);
        //    if (redirect == null)
        //        return NotFound("Ad not found.");

        //    return Redirect(redirect);
        //}
        //// ==========================
        //// 🔹 GET: /ad/impression?adId=5&key=xxxxx
        //// ==========================
        //[HttpGet("impression")]
        //[AllowAnonymous]
        //public async Task<IActionResult> RegisterImpression([FromQuery] int adId, [FromQuery] string key)
        //{
        //    // Validate ad exists
        //    var adExists = await _context.Ads.AnyAsync(a => a.Id == adId);
        //    if (!adExists) return NotFound("Ad not found.");

        //    // resolve website hostDomain if key provided
        //    var website = string.IsNullOrEmpty(key) ? null : await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key);
        //    string? hostDomain = website?.Domain;

        //    string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        //    string? ua = Request.Headers["User-Agent"].ToString();

        //    await _adService.RecordImpressionAsync(adId, hostDomain, ip, ua);

        //    // return 204 No Content to indicate success and avoid unnecessary payload
        //    return NoContent();
        //}


    }
}
