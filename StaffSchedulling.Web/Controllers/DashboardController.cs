using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Services.DbServices.Contracts;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class DashboardController(ICompanyService _companyService) : Controller
    {

        //Get all companies - Owned and Joined
        [HttpGet]
        public async Task<IActionResult> Index(string? sortFilter)
        {
            string? currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";

            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(currentUserEmail);

            //Set default sortFilter to 'Name Ascending' and change it up if sort is different
            ViewData["SortFilter"] = String.IsNullOrEmpty(sortFilter) ? "NameAsc" : sortFilter;

            return View(model);
        }
    }
}
