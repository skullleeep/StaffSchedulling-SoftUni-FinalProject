﻿using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    public class EmployeeController(IPermissionService _permissionService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
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
                return RedirectToAction("Index", "Dashboard");
            }

            //Add employee
            StatusReport status = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

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

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Change employee's role
            StatusReport status = await _employeeInfoService.ChangeRoleAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

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

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Change employee's role
            StatusReport status = await _employeeInfoService.ChangeDepartmentAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

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

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Delete employee
            StatusReport status = await _employeeInfoService.DeleteEmployeeAsync(model, permissionRole);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;

                return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

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
                return RedirectToAction("Index", "Dashboard");
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
