﻿using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    public class EmployeeController(IEmployeeInfoService _employeeInfoService) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> AddEmployeeManually(AddEmployeeInfoManuallyInputModel model)
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

            PermissionRole role = await _employeeInfoService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (role < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Add employee
            StatusReport status = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["EmployeeError"] = status.Message;
            }

            return RedirectToAction("Employees", "Manage", new { id = model.CompanyId });
        }
    }
}
