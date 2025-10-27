//using Microsoft.AspNetCore.Mvc;
//using AdSystem.Data;
//using System.Linq;
//using System.Text;
//using Microsoft.EntityFrameworkCore;

//namespace AdSystem.Controllers
//{
//    [Route("script")]
//    public class AdScriptController : Controller
//    {
//        private readonly AppDbContext _context;

//        public AdScriptController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("{websiteId}.js")]
//        public IActionResult GetScript(int websiteId)
//        {
//            // MVP: select a random active ad for the requesting website.
//            // Note: OrderBy(Guid.NewGuid()) is acceptable for small datasets for MVP.
//            var ad = _context.Ads
//                .Where(a => a.IsActive)
//                .OrderBy(a => Guid.NewGuid())
//                .FirstOrDefault();

//            if (ad == null)
//                return Content("// No ads available", "application/javascript");

//            var sb = new StringBuilder();

//            // The script will render the ad and call the tracking endpoints
//            sb.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
//            sb.AppendLine($@"
//    (function() {{
//        try {{
//            var adDiv = document.createElement('div');
//            adDiv.style.border = '1px solid #ccc';
//            adDiv.style.padding = '10px';
//            adDiv.style.margin = '10px';
//            adDiv.style.textAlign = 'center';
//            adDiv.innerHTML = `<a href='{ad.TargetUrl}' target='_blank' rel='noopener noreferrer'>
//                <img src='{ad.ImageUrl}' alt='{EscapeJs(ad.Title)}' style='max-width:200px; display:block; margin:auto;'>
//                <p>{EscapeJs(ad.Title)}</p>
//            </a>`;
//            document.body.appendChild(adDiv);

//            // Track impression (fire-and-forget)
//            fetch('{Url.Action("TrackImpression", "AdScript", new { id = ad.Id }, Request.Scheme)}', {{ method: 'POST', mode: 'no-cors' }});

//            var anchor = adDiv.querySelector('a');
//            if (anchor) {{
//                anchor.addEventListener('click', function() {{
//                    // Track click then allow default navigation
//                    try {{
//                        navigator.sendBeacon('{Url.Action("TrackClick", "AdScript", new { id = ad.Id }, Request.Scheme)}');
//                    }} catch(e) {{
//                        // fallback
//                        fetch('{Url.Action("TrackClick", "AdScript", new { id = ad.Id }, Request.Scheme)}', {{ method: 'POST', mode: 'no-cors' }});
//                    }}
//                }});
//            }}
//        }} catch (e) {{
//            console && console.error(e);
//        }}
//    }})();");

//            sb.AppendLine("});");

//            Response.Headers["Cache-Control"] = "public, max-age=60"; // short cache to help rotate ads
//            Response.Headers["Access-Control-Allow-Origin"] = "*";

//            return Content(sb.ToString(), "application/javascript; charset=utf-8");
//        }
//        // Utility: very small helper to escape backticks etc (server-side)
//        private static string EscapeJs(string? s)
//        {
//            if (string.IsNullOrEmpty(s)) return "";
//            return s.Replace("\\", "\\\\").Replace("`", "\\`").Replace("\r", "").Replace("\n", " ");
//        }

//        [HttpPost("track-impression/{id}")]
//        public IActionResult TrackImpression(int id)
//        {
//            var ad = _context.Ads.Find(id);
//            if (ad == null) return NotFound();

//            ad.Impressions++;
//            _context.SaveChanges();

//            return Ok();
//        }

//        [HttpPost("track-click/{id}")]
//        public IActionResult TrackClick(int id)
//        {
//            var ad = _context.Ads.Find(id);
//            if (ad == null) return NotFound();

//            ad.Clicks++;
//            _context.SaveChanges();

//            return Ok();
//        }
//        [HttpGet("debug-db")]
//        //quick diagnostic test:
//        public IActionResult DebugDb()
//        {
//            var cs = _context.Database.GetDbConnection().ConnectionString;
//            var count = _context.Ads.Count();
//            var ad = _context.Ads.FirstOrDefault();
//            return Content($"Connection: {cs}\nTotal Ads: {count}\nAd1 Impressions: {ad?.Impressions}, Clicks: {ad?.Clicks}");
//        }


//    }
//}
