using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Controllers;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Constants.ApplicationConstants;

namespace StaffScheduling.Web.Areas.Administration.Controllers
{
    [Area(AdministrationAreaName)]
    [Authorize(Roles = AdministrationRoleName)]
    public class DashboardController(ICompanyService _companyService) : BaseController
    {

        //Get all companies
        [HttpGet]
        public async Task<IActionResult> Index(string? searchQuery, CompanySortFilter? sortFilter, int page = 1)
        {
            var model = await _companyService.GetCompaniesForAdministratorModel(searchQuery, sortFilter, page);

            return View(model);
        }
    }
}
