//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Identity;
//using AdSystem.Models;

//namespace AdSystem.Filters
//{
//    public class ActiveUserFilter : IAsyncActionFilter
//    {
//        private readonly UserManager<ApplicationUser> _userManager;

//        public ActiveUserFilter(UserManager<ApplicationUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            // Skip if user is not authenticated
//            if (!context.HttpContext.User.Identity.IsAuthenticated)
//            {
//                await next();
//                return;
//            }

//            var user = await _userManager.GetUserAsync(context.HttpContext.User);

//            if (user != null && !user.IsActive)
//            {
//                // User is not active, sign them out and redirect
//                context.Result = new RedirectToActionResult("AccountDeactivated", "Account", null);
//                return;
//            }

//            await next();
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using AdSystem.Models;

namespace AdSystem.Filters
{
    public class ActiveUserFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ActiveUserFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip if user is not authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                await next();
                return;
            }

            // Get current controller and action
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            // Skip the AccountDeactivated action itself to avoid infinite loop
            if (controllerName?.ToLower() == "account" && actionName?.ToLower() == "accountdeactivated")
            {
                await next();
                return;
            }

            // Skip Identity pages (login, logout, etc.)
            var path = context.HttpContext.Request.Path.Value?.ToLower();
            if (path?.StartsWith("/identity") == true)
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user != null && !user.IsActive)
            {
                // User is not active, redirect to deactivated page
                context.Result = new RedirectToActionResult("AccountDeactivated", "Account", null);
                return;
            }

            await next();
        }
    }
}