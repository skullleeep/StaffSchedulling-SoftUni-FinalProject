using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class CompanyController(ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        //Get request when trying to join a company
        [HttpGet("[controller]/[action]/{inviteCode?}")]
        public async Task<IActionResult> Join(string? inviteCode)
        {
            Guid inviteGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(inviteCode, ref inviteGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _companyService.GetCompanyFromInviteLinkAsync(inviteGuid);

            //Check for wrong id
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        //Post request when trying to join a company
        [HttpPost("[controller]/[action]/{inviteCode}")]
        public async Task<IActionResult> Join(CompanyJoinViewModel model, string inviteCode)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Get user mail
            string userEmail = GetCurrentUserEmail();

            //Get user email
            string userId = GetCurrentUserId();

            //Join company
            StatusReport status = await _employeeInfoService.JoinCompanyWithIdAsync(model.Id, userId, userEmail);

            //Check for errors
            if (status.Ok == false)
            {
                ModelState.AddModelError(String.Empty, status.Message);
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //Get request when trying to create a company
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CompanyCreateInputModel();

            return View(model);
        }

        //Post request when trying to create a company
        [HttpPost]
        public async Task<IActionResult> Create(CompanyCreateInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userId = GetCurrentUserId();

            //Create company
            StatusReport status = await _companyService.CreateCompanyAsync(model, userId);

            //Check for errors
            if (status.Ok == false)
            {
                ModelState.AddModelError(String.Empty, status.Message);
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //Get request when trying to edit a company
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _employeeInfoService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _companyService.GetCompanyEditInputModelAsync(companyGuid);

            return View(model);
        }

        //Post request when trying to edit a company
        [HttpPost]
        public async Task<IActionResult> Edit(CompanyEditInputModel model, string id)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _employeeInfoService.GetUserPermissionInCompanyAsync(model.Id, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            string userId = GetCurrentUserId();

            //Edit company
            StatusReport status = await _companyService.EditCompanyAsync(model, userId);

            //Check for errors
            if (status.Ok == false)
            {
                ModelState.AddModelError(String.Empty, status.Message);
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //Post request when trying to delete a company
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _employeeInfoService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Owner)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Delete company
            StatusReport status = await _companyService.DeleteCompanyAsync(companyGuid);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["DeleteError"] = status.Message;
                return RedirectToAction("Company", "Manage", new { id = id });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
