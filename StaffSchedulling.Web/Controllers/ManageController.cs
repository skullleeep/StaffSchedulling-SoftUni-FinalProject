using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class ManageController(ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Company(string id)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole role = await _employeeInfoService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (role < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }


            bool canUserEdit = role >= PermissionRole.Editor ? true : false; //Check for edit permission
            bool canUserDelete = role == PermissionRole.Owner ? true : false; //Check for delete permission

            var model = await _companyService.GetCompanyFromIdAsync(companyGuid, canUserEdit, canUserDelete);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Employees(string id, string? searchQuery, EmployeeSearchFilter? searchFilter)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole role = await _employeeInfoService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (role < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(companyGuid, searchQuery, searchFilter);

            return View(model);
        }
    }
}
