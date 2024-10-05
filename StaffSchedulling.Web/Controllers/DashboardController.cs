using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Services.DbServices.Contracts;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class DashboardController(ICompanyService _companyService) : Controller
    {

        public async Task<IActionResult> Index()
        {
            string? currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(currentUserEmail);

            return View(model);
        }
    }
}
