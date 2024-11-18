using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class ManageController(ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        [HttpGet("Manage/Company/{id?}")]
        public async Task<IActionResult> Company(string id)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail() ?? String.Empty;

            PermissionRole role = await _employeeInfoService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (role < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }


            bool canUserEdit = role >= PermissionRole.Manager ? true : false; //Check for edit permission
            bool canUserDelete = role == PermissionRole.Owner ? true : false; //Check for delete permission

            var model = await _companyService.GetCompanyFromIdAsync(companyGuid, canUserEdit, canUserDelete);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}
