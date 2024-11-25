using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class DashboardController(ICompanyService _companyService) : BaseController
    {

        //Get all companies - Owned and Joined
        [HttpGet]
        public async Task<IActionResult> Index(CompanySortFilter? sortFilter)
        {
            string currentUserEmail = GetCurrentUserEmail();

            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(currentUserEmail, sortFilter);

            return View(model);
        }
    }
}
