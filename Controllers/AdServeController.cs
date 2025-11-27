using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using AdSystem.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Controllers
{
    [ApiController]
    [EnableCors("AdScriptPolicy")] // Applies strict CORS
    public class AdServeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdServeController> _logger;
        private readonly IFinanceService _financeService;
        private readonly IGeoLocationService _geoService;


        public AdServeController(AppDbContext context, ILogger<AdServeController> logger, IFinanceService financeService, IGeoLocationService geoService)
        {
            _context = context;
            _logger = logger;
            _financeService = financeService;
            _geoService = geoService;
        }

        [HttpGet("ad.js")]
        public async Task<IActionResult> ServeAdScript([FromQuery] string key)
        {
            //Response.Headers.Add("Access-Control-Allow-Origin", "*");
            var website = await _context.Websites
                .Include(w => w.Placements)
                .FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
                return Content("// Invalid or unapproved website key", "application/javascript");
            var origin = Request.Headers["Origin"].FirstOrDefault() ??
                         Request.Headers["Referer"].FirstOrDefault();
            Console.WriteLine("1#########");
            Console.WriteLine("serving ad.js for key: " + key + ", origin: " + origin);
            if (!IsValidDomain(origin, website.Domain))
                return Content("// Unauthorized domain", "application/javascript");

            // ✅ Make absolute base URL (important for ads on other domains)
            var apiBase = $"{Request.Scheme}://{Request.Host}";
            var js = $@"
        (function() {{
          const placements = document.querySelectorAll('[data-zone]');
          placements.forEach(p => {{
            const zone = p.getAttribute('data-zone');
            fetch(`{apiBase}/ad/fetch?zone=${{zone}}&key={key}`)
              .then(r => r.json())
              .then(ad => {{
                if (!ad.imagePath) return;

                // ✅ Detect ad type based on dimensions
                const isLeaderboard = ad.width >= 468 && ad.height <= 90; // 728x90, 468x60
                const isSkyscraper = ad.height >= 600; // 160x600
                const isRectangle = ad.width <= 336 && ad.height >= 250 && ad.height <= 280; // 300x250, 336x280
                const isBanner = ad.width >= 468 && ad.height === 60; // 468x60

                // ✅ Get parent container width
                const parentWidth = p.parentElement ? p.parentElement.offsetWidth : window.innerWidth;
                const adRatio = ad.height / ad.width;

                // ✅ Calculate responsive dimensions
                let displayWidth = ad.width;
                let displayHeight = ad.height;

                if (displayWidth > parentWidth) {{
                  displayWidth = parentWidth;
                  displayHeight = Math.round(displayWidth * adRatio);
                }}

                // ✅ Smart container styling based on ad type
                let containerStyle = '';

                if (isSkyscraper) {{
                  // Skyscraper: float right/left, content flows around it
                  containerStyle = `
                    display: block;
                    width: ${{ad.width}}px;
                    max-width: 100%;
                    float: right;
                    margin: 8px 0 16px 16px;
                    clear: right;
                  `;
                }} else if (isLeaderboard || isBanner) {{
                  // Leaderboard/Banner: full width, centered, no float
                  containerStyle = `
                    display: block;
                    max-width: ${{displayWidth}}px;
                    width: 100%;
                    margin: 16px auto;
                    clear: both;
                  `;
                }} else if (isRectangle) {{
                  // Rectangle: can float or center based on parent
                  const parentIsWide = parentWidth > 768;
                  if (parentIsWide) {{
                    containerStyle = `
                      display: block;
                      width: ${{ad.width}}px;
                      max-width: 100%;
                      float: right;
                      margin: 8px 0 16px 16px;
                      clear: right;
                    `;
                  }} else {{
                    containerStyle = `
                      display: block;
                      max-width: ${{displayWidth}}px;
                      width: 100%;
                      margin: 16px auto;
                      clear: both;
                    `;
                  }}
                }} else {{
                  // Default: centered
                  containerStyle = `
                    display: block;
                    max-width: ${{displayWidth}}px;
                    width: 100%;
                    margin: 16px auto;
                    clear: both;
                  `;
                }}

                // ✅ Apply base styles + smart positioning
                p.style.cssText = containerStyle + `
                  text-align: center;
                  overflow: visible;
                  position: relative;
                  box-sizing: border-box;
                  line-height: 0;
                `;

                // ✅ Responsive ad content with proper aspect ratio
                p.innerHTML = `
                  <div style=""
                    position: relative;
                    width: 100%;
                    max-width: ${{ad.width}}px;
                    margin: 0 auto;
                    padding-bottom: ${{(adRatio * 100).toFixed(2)}}%;
                    background: #f9f9f9;
                    border-radius: 4px;
                    overflow: hidden;
                    box-shadow: 0 1px 4px rgba(0,0,0,0.1);
                  "">
                    <a href=""{apiBase}/ad/click?adId=${{ad.id}}&key={key}"" 
                       target=""_blank"" 
                       rel=""noopener noreferrer""
                       style=""
                         position: absolute;
                         top: 0;
                         left: 0;
                         width: 100%;
                         height: 100%;
                         display: block;
                       "">
                      <img 
                        src=""${{ad.imagePath}}"" 
                        alt=""Advertisement""
                        loading=""lazy""
                        style=""
                          position: absolute;
                          top: 0;
                          left: 0;
                          width: 100%;
                          height: 100%;
                          object-fit: cover;
                          background: #fff;
                        ""
                      />
                    </a>
                  </div>
                `;

                // ✅ Track impression
                fetch(`{apiBase}/ad/impression?adId=${{ad.id}}&key={key}`, {{ method: 'POST' }});
              }})
              .catch(err => {{
                console.warn('Ad loading failed:', err);
                p.style.display = 'none';
              }});
          }});

          // ✅ Handle window resize for responsive ads
          let resizeTimeout;
          window.addEventListener('resize', function() {{
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function() {{
              placements.forEach(p => {{
                const container = p.querySelector('div');
                const img = p.querySelector('img');
                if (container && img) {{
                  const parentWidth = p.parentElement ? p.parentElement.offsetWidth : window.innerWidth;

                  // Re-detect ad type from container dimensions
                  const adWidth = parseInt(container.style.maxWidth) || 300;
                  const isSkyscraper = container.offsetHeight >= 600;
                  const isLeaderboard = adWidth >= 468;

                  // Adjust float behavior on mobile
                  if (parentWidth < 768 && (isSkyscraper || !isLeaderboard)) {{
                    p.style.float = 'none';
                    p.style.margin = '16px auto';
                    p.style.maxWidth = parentWidth + 'px';
                    container.style.maxWidth = parentWidth + 'px';
                  }}
                }}
              }});
            }}, 250);
          }});
        }})();
        ";
            return Content(js, "application/javascript");
        }

        [HttpGet("ad/fetch")]
        public async Task<IActionResult> FetchAd([FromQuery] string zone, [FromQuery] string key)
        {
            //Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var website = await _context.Websites
                .Include(w => w.Placements)
                .Include(w => w.Placements)
                .FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);

            if (website == null)
                return BadRequest(new { error = "Invalid key" });

            var placement = website.Placements?.FirstOrDefault(p => p.ZoneKey == zone);
            if (placement == null)
                return Ok(new { });

            var ads = await _context.Ads
                .Include(a => a.BannerSize)
                .Include(a => a.Advertiser)
                .Where(a => a.Status == AdStatus.Approved
                    && a.Advertiser != null
                    && a.Advertiser.Balance > 0
                    && a.BannerSizeId == placement.BannerSizeId)
                .ToListAsync();

            if (!ads.Any())
                return Ok(new { });

            var ad = ads.OrderBy(a => Guid.NewGuid()).First();

            //var absoluteImage = $"{Request.Scheme}://{Request.Host}{ad.ImagePath}";
            // If the path is already a full URL (Cloudinary), use it as-is
            var absoluteImage = ad.ImagePath.StartsWith("http")
                ? ad.ImagePath
                : $"{Request.Scheme}://{Request.Host}{ad.ImagePath}";


            return Ok(new
            {
                id = ad.Id,
                imagePath = absoluteImage,
                width = ad.BannerSize.Width,
                height = ad.BannerSize.Height,
                target = ad.TargetUrl
            });
        }

        private string GetClientIp()
        {
            var headers = HttpContext.Request.Headers;
            string? ip = headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(ip))
                ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // If multiple IPs (e.g., "203.0.113.1, 70.41.3.18"), take the first one
            if (!string.IsNullOrEmpty(ip) && ip.Contains(","))
                ip = ip.Split(',')[0].Trim();

            return string.IsNullOrWhiteSpace(ip) ? "Unknown" : ip;
        }

        [HttpPost("ad/impression")]
        public async Task<IActionResult> RecordImpression([FromQuery] int adId, [FromQuery] string key)
        {
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
                return BadRequest("Invalid or unapproved website key.");

            var origin = Request.Headers["Origin"].FirstOrDefault() ?? Request.Headers["Referer"].FirstOrDefault();
            Console.WriteLine("2#########");
            Console.WriteLine("serving recordImpression for key: " + key + ", origin: " + origin);
            if (!IsValidDomain(origin, website.Domain))
                return BadRequest("Unauthorized domain for this website key.");

            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == adId && a.Status == AdStatus.Approved);
            if (ad == null)
                return NotFound("Ad not found or inactive.");

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            //var ip = "3.84.174.52";
            var (country, city) = await _geoService.ResolveIpAsync(ip);

            var impression = new AdImpression
            {
                AdId = adId,
                WebsiteId = website.Id,
                HostDomain = website.Domain,
                IPAddress = ip,
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Country = country ?? "Unknown",
                City = city ?? "Unknown",
                ViewedAt = DateTime.UtcNow
            };

            _context.AdImpressions.Add(impression);

            try
            {
                await _financeService.HandleImpressionAsync(impression);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finance failed for impression {ImpressionId}", impression.Id);
            }

            return Ok(new { success = true });
        }

        [HttpGet("ad/click")]
        public async Task<IActionResult> RecordClick([FromQuery] int adId, [FromQuery] string key)
        {
            var website = await _context.Websites.FirstOrDefaultAsync(w => w.ScriptKey == key && w.IsApproved);
            if (website == null)
                return BadRequest("Invalid or unapproved website key.");

            //var origin = Request.Headers["Origin"].FirstOrDefault() ?? Request.Headers["Referer"].FirstOrDefault();
            //Console.WriteLine("3#########");
            //Console.WriteLine("serving RecordClick for key: " + key + ", origin: " + origin);
            //Console.WriteLine("#################O:"+Request.Headers["Origin"]);
            //Console.WriteLine("-----------------R:"+Request.Headers["Referer"]);
            //if (!IsValidDomain(origin, website.Domain))
            //    return BadRequest("Unauthorized domain for this website key.");

            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == adId && a.Status == AdStatus.Approved);
            if (ad == null)
                return NotFound("Ad not found or inactive.");

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (country, city) = await _geoService.ResolveIpAsync(ip);

            var click = new AdClick
            {
                AdId = adId,
                WebsiteId = website.Id,
                IPAddress = ip,
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Country = country ?? "Unknown",
                City = city ?? "Unknown",
                ClickedAt = DateTime.UtcNow
            };

            _context.AdClicks.Add(click);

            try
            {
                await _financeService.HandleClickAsync(click);
                 await _context.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finance failed for click {ClickId}", click.Id);
            }

            return Redirect(ad.TargetUrl);
        }

        private static bool IsValidDomain(string? origin, string websiteDomain)
        {
            if (origin == null) return false;
            try
            {
                var originHost = new Uri(origin).Host.Replace("www.", "").ToLower();
                var targetHost = new UriBuilder(websiteDomain).Uri.Host.Replace("www.", "").ToLower();
                return originHost.EndsWith(targetHost);
            }
            catch
            {
                return origin.Contains(websiteDomain, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
