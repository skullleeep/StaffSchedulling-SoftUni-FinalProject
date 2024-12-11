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
        //Post request when an employee wants to add a department in a company where he has managing rights
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
                return Forbid();
            }

            //Add department
            StatusReport status = await _departmentService.AddDepartmentManuallyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DepartmentError"] = status.Message;

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Departments", "Manage", new { id = model.CompanyId }); //No scrollToTable because we don't want that after adding
        }

        //Post request when an employee wants to delete a department in a company where he has managing rights
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

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Delete department
            StatusReport status = await _departmentService.DeleteDepartmentAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DepartmentError"] = status.Message;

                return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, model.CurrentPage });
            }

            return RedirectToAction("Departments", "Manage", new { id = model.CompanyId, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete all departments in a company where he has managing rights
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
                return Forbid();
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
