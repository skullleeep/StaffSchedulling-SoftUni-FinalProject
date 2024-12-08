using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.Vacation;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    public class VacationController(IPermissionService _permissionService, IVacationService _vacationService) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> AddVacationOfEmployee(AddVacationOfEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Visitor || permissionRole == PermissionRole.Owner) //Don't allow owner as the owner doesn't have a schedule
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user id
            string userId = GetCurrentUserId();

            //Add vacation
            StatusReport status = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVacationOfEmployee(DeleteVacationOfEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Visitor || permissionRole == PermissionRole.Owner) //Don't allow owner as the owner doesn't have a schedule
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user id
            string userId = GetCurrentUserId();

            //Remove vacation
            StatusReport status = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllVacationsOfEmployee(DeleteAllVacationsOfEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Visitor || permissionRole == PermissionRole.Owner) //Don't allow owner as the owner doesn't have a schedule
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user id
            string userId = GetCurrentUserId();

            //Remove vacation
            StatusReport status = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }
    }
}
