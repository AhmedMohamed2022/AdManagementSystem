using AdManagementSystem.Models.Enums;
using AdSystem.Models;
using AdSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Controllers
{
    [ApiController]
    public class AdServeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdServeController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 GET: /ad.js?key=xxxx
        [HttpGet("ad.js")]
        public async Task<IActionResult> ServeAdScript([FromQuery] string key)
        {
            // 1️⃣ Validate the website key
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
            {
                return Content("// Invalid or unapproved website key", "application/javascript");
            }

            // 2️⃣ Pick a random approved ad
            var ad = await _context.Ads
                .Where(a => a.Status == AdStatus.Approved)
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (ad == null)
            {
                return Content("// No active ads available", "application/javascript");
            }

            // 3️⃣ Build the dynamic JS
            var js = $@"
(function() {{
    const container = document.getElementById('ad-container');
    if (!container) return;

    container.innerHTML = `<a href='https://{Request.Host}/ad/click?adId={ad.Id}&key={key}' target='_blank'>
        <img src='{ad.ImageUrl}' alt='{ad.Title}' style='max-width:100%;border-radius:8px;cursor:pointer;'/>
    </a>`;

    // Track impression asynchronously
    fetch('https://{Request.Host}/ad/impression?adId={ad.Id}&key={key}', {{ method: 'POST' }})
        .catch(err => console.warn('Ad impression tracking failed', err));
}})();
";

            // 4️⃣ Return as JS content
            return Content(js, "application/javascript");
        }
        // 🔹 POST: /ad/impression?adId=3&key=xxxx
        [HttpPost("ad/impression")]
        public async Task<IActionResult> RecordImpression([FromQuery] int adId, [FromQuery] string key)
        {
            // 1️⃣ Validate the website
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
                return BadRequest("Invalid or unapproved website key.");

            // 2️⃣ Validate the ad
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == adId && a.Status == AdStatus.Approved);
            if (ad == null)
                return NotFound("Ad not found or not active.");

            // 3️⃣ Create a new impression record
            var impression = new AdImpression
            {
                AdId = adId,
                HostDomain = Request.Headers["Origin"].FirstOrDefault() ?? website.Domain,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                ViewedAt = DateTime.UtcNow,
                WebsiteId=website.Id                
            };

            _context.AdImpressions.Add(impression);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        // 🔹 GET: /ad/click?adId=3&key=xxxx
        [HttpGet("ad/click")]
        public async Task<IActionResult> RecordClick([FromQuery] int adId, [FromQuery] string key)
        {
            // 1️⃣ Validate the website
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
                return BadRequest("Invalid or unapproved website key.");

            // 2️⃣ Validate the ad
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == adId && a.Status == AdStatus.Approved);
            if (ad == null)
                return NotFound("Ad not found or not active.");

            // 3️⃣ Create a click record
            var click = new AdClick
            {
                AdId = adId,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                ClickedAt = DateTime.UtcNow,
                WebsiteId=website.Id,

            };

            _context.AdClicks.Add(click);
            await _context.SaveChangesAsync();

            // 4️⃣ Redirect user to the advertiser's target URL
            return Redirect(ad.TargetUrl);
        }

    }
}
