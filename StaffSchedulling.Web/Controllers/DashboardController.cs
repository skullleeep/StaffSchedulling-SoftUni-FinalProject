using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Constants.ApplicationConstants;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class DashboardController(ICompanyService _companyService) : BaseController
    {

        //Get all companies - Owned and Joined
        [HttpGet]
        public async Task<IActionResult> Index(CompanySortFilter? sortFilter)
        {
            if (User.IsInRole(UserRole.Administrator.ToString()))
            {
                return RedirectToAction("Index", "Dashboard", new { area = AdministrationAreaName });
            }

            string currentUserEmail = GetCurrentUserEmail();

            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(currentUserEmail, sortFilter);

            return View(model);
        }
    }
}
