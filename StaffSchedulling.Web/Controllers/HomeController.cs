using Microsoft.AspNetCore.Mvc;

namespace StaffScheduling.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController()
        {
        }

        //Check for user authentication and if not continue normally else send user to dashboard
        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (!statusCode.HasValue)
            {
                return View();
            }

            if (statusCode == 404 || statusCode == 403 || statusCode == 401)
            {
                return View($"Error{statusCode}");
            }

            return View("Error500");
        }
    }
}
