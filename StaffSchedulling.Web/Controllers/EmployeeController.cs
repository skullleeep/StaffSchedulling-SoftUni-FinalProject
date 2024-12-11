using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    public class EmployeeController(IPermissionService _permissionService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        //Post request when an employee wants to add another employee in a company where he has managing rights
        [HttpPost]
        public async Task<IActionResult> AddManually(AddEmployeeInfoManuallyInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["EmployeeError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Add employee
            StatusReport status = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId }); //No scrollToTable because we don't want that after adding
        }

        //Post request when an employee wants to change an emoloyee's Role in a company where he has managing rights
        [HttpPost]
        public async Task<IActionResult> ChangeRole(ChangeRoleInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["EmployeeError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Change employee's role
            StatusReport status = await _employeeInfoService.ChangeRoleAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to change an emoloyee's Department in a company where he has managing rights
        [HttpPost]
        public async Task<IActionResult> ChangeDepartment(ChangeDepartmentInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["EmployeeError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Change employee's role
            StatusReport status = await _employeeInfoService.ChangeDepartmentAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete another employee in a company where he has managing rights
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["EmployeeError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Delete employee
            StatusReport status = await _employeeInfoService.DeleteEmployeeAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SearchFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete all other employees in a company where he has managing rights
        [HttpPost]
        public async Task<IActionResult> DeleteAll(DeleteAllEmployeesInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["EmployeeError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            //Delete all employees
            StatusReport status = await _employeeInfoService.DeleteAllEmployeesAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }
    }
}
