﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class CompanyController(IPermissionService _permissionService, ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        //Get request when user is trying to join a company
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

        //Post request when user is trying to join a company
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

        //Get request when user is trying to create a company
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CompanyCreateInputModel();

            return View(model);
        }

        //Post request when user is trying to create a company
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

        //Get request when employee is trying to edit a company in which he has managing rights
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

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
            }

            var model = await _companyService.GetCompanyEditInputModelAsync(companyGuid);

            return View(model);
        }

        //Get request when employee is trying to edit a company in which he has managing rights
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

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.Id, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return Forbid();
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

            return RedirectToAction("Company", "Manage", new { id = model.Id.ToString() });
        }

        //Post request when employee is trying to delete a company in which he has managing rights
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteCompanyInputModel model)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                //Get model errors
                string message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["CompanyError"] = String.Format(ModelErrorMessages.InvalidModelStateFormat, message);

                return RedirectToAction("Company", "Manage", new { id = model.CompanyId });
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(model.CompanyId, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Owner)
            {
                return Forbid();
            }

            //Delete company
            StatusReport status = await _companyService.DeleteCompanyAsync(model);

            //Check for errors
            if (status.Ok == false)
            {
                TempData["CompanyError"] = status.Message;
                return RedirectToAction("Company", "Manage", new { id = model.CompanyId });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
