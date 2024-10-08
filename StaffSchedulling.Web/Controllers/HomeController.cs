using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Models;
using System.Diagnostics;

namespace StaffScheduling.Web.Controllers
{
    public class HomeController : Controller
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
        public async Task<IActionResult> Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
