using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.Department;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    public class DepartmentController(IPermissionService _permissionService, IDepartmentService _departmentService) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> AddManually(AddDepartmentManuallyInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["DepartmentError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Add department
            StatusReport status = await _departmentService.AddDepartmentManuallyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DepartmentError"] = status.Message;

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteDepartmentInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["DepartmentError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Delete department
            StatusReport status = await _departmentService.DeleteDepartmentAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DepartmentError"] = status.Message;

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll(DeleteAllDepartmentsInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["DepartmentError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Delete all departments
            StatusReport status = await _departmentService.DeleteAllDepartmentsAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DepartmentError"] = status.Message;

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }
    }
}
