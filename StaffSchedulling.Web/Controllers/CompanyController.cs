using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class CompanyController(ICompanyService _companyService, IEmployeeInfoService _employeeInfoService) : Controller
    {
        //Get request when trying to join a company
        [HttpGet("Company/Join/{inviteCode?}")]
        public async Task<IActionResult> Join(string? inviteCode)
        {
            if (inviteCode == null)
            {
                return RedirectToAction(nameof(Index), "Dashboard");
            }

            Guid inviteGuid = Guid.Empty;
            if (Guid.TryParse(inviteCode, out inviteGuid) == false)
            {
                return RedirectToAction(nameof(Index), "Dashboard");
            }

            var model = await _companyService.GetCompanyFromInviteLinkAsync(inviteGuid);
            if (model == null)
            {
                return RedirectToAction(nameof(Index), "Dashboard");
            }

            return View(model);
        }

        //Post request when trying to join a company
        [HttpPost("Company/Join/{inviteCode}")]
        public async Task<IActionResult> Join(CompanyViewModel model, string inviteCode)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hasCompany = await _companyService.HasCompanyWithIdAsync(model.Id);
            if (hasCompany == false)
            {
                ModelState.AddModelError(String.Empty, "Couldn't find company!");
                return View(model);
            }

            string companyOwnerEmail = await _companyService.GetCompanyOwnerEmailFromIdAsync(model.Id);
            string currentUserId = GetCurrentUserId() ?? "";
            StatusReport status = await _employeeInfoService.JoinCompanyWithIdAsync(model.Id, companyOwnerEmail, currentUserId);
            if (status.Ok == false)
            {
                ModelState.AddModelError(String.Empty, status.Message);
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //Get request when trying to join a company
        //[HttpGet("Company/Create/{companyName?}")]
        [HttpGet]
        public async Task<IActionResult> Create(string? companyName)
        {

            var model = new CompanyCreateFormModel();

            if (!String.IsNullOrEmpty(companyName))
            {
                model.Name = companyName;
            }

            return View(model);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
