using Microsoft.AspNetCore.Mvc;

namespace AdSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
