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
        //Post request when an employee wants to add vacation request to his schedule
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

                TempData["ScheduleError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

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
                TempData["ScheduleError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId }); //No scrollToTable because we don't want that after adding
        }

        //Post request when an employee wants to delete a vacation request from his schedule
        [HttpDelete]
        public async Task<IActionResult> DeleteVacationOfEmployee(DeleteVacationOfEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["ScheduleError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, model.SortFilter, model.CurrentPage });
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
                TempData["ScheduleError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, model.SortFilter, model.CurrentPage });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, model.SortFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete all vacation requests with chosen status from his schedule
        [HttpDelete]
        public async Task<IActionResult> DeleteAllVacationsOfEmployee(DeleteAllVacationsOfEmployeeInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["ScheduleError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

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
                TempData["ScheduleError"] = status.Message;

                return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Schedule", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to change status of a vacation request in a company where he has managing rights
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(ChangeStatusInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Check if the user's employee role needs to have a department
            //And if it does just get it
            Guid? userNeededDepartmentId = await _permissionService.GetUserNeededDepartmentId(model.CompanyId, userEmail);

            //Change vacation status
            StatusReport status = await _vacationService.ChangeStatusAsync(model, permissionRole, userNeededDepartmentId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage });
            }

            return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete a vacation request in a company where he has managing rights
        [HttpDelete]
        public async Task<IActionResult> DeleteVacationOfCompany(DeleteVacationOfCompanyInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Check if the user's employee role needs to have a department
            //And if it does just get it
            Guid? userNeededDepartmentId = await _permissionService.GetUserNeededDepartmentId(model.CompanyId, userEmail);

            //Remove vacation
            StatusReport status = await _vacationService.DeleteVacationOfCompanyAsync(model, permissionRole, userNeededDepartmentId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage });
            }

            return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, model.SearchQuery, model.SortFilter, model.CurrentPage, scrollToTable = true }); //scrollToTable detected by javascript
        }

        //Post request when an employee wants to delete all vacation requests with chosen status in a company where he has managing rights
        [HttpDelete]
        public async Task<IActionResult> DeleteAllVacationsOfCompany(DeleteAllVacationsOfCompanyInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["VacationError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Check if the user's employee role needs to have a department
            //And if it does just get it
            Guid? userNeededDepartmentId = await _permissionService.GetUserNeededDepartmentId(model.CompanyId, userEmail);

            //Remove all vacations up to status
            StatusReport status = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, permissionRole, userNeededDepartmentId);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["VacationError"] = status.Message;

                return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Vacations", "Manage", new { id = model.CompanyId, scrollToTable = true }); //scrollToTable detected by javascript
        }
    }
}
