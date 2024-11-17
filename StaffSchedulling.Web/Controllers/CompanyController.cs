using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class CompanyController(ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : BaseController
    {
        //Get request when trying to join a company
        [HttpGet("Company/Join/{inviteCode?}")]
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
        [HttpPost("Company/Join/{inviteCode}")]
        public async Task<IActionResult> Join(CompanyViewModel model, string inviteCode)
        {
            //Check for model errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hasCompany = await _companyService.HasCompanyWithIdAsync(model.Id);

            //Check if company id is wrong
            if (hasCompany == false)
            {
                ModelState.AddModelError(String.Empty, "Couldn't find company!");
                return View(model);
            }

            string companyOwnerEmail = await _companyService.GetCompanyOwnerEmailFromIdAsync(model.Id);
            string currentUserId = GetCurrentUserId() ?? String.Empty;

            //Join company
            StatusReport status = await _employeeInfoService.JoinCompanyWithIdAsync(model.Id, companyOwnerEmail, currentUserId);

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

            string userId = GetCurrentUserId() ?? String.Empty;

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
    }
}
