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
            Guid companyId = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyId) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail() ?? String.Empty;

            //Get company owner email and at the same time check if company id is wrong
            string companyOwnerEmail = await _companyService.GetCompanyOwnerEmailFromIdAsync(companyId) ?? String.Empty;

            if (String.IsNullOrEmpty(companyOwnerEmail))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            PermissionRole role = await _employeeInfoService.GetRoleOfEmployeeInCompanyAsync(companyId, companyOwnerEmail, userEmail);

            //Check for access permission
            if (role < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }


            bool canUserEdit = role >= PermissionRole.Manager ? true : false; //Check for edit permission
            bool canUserDelete = role == PermissionRole.Owner ? true : false; //Check for delete permission

            var model = await _companyService.GetCompanyFromIdAsync(companyId, canUserEdit, canUserDelete);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}
