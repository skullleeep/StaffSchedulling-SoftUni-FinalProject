using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Services.DbServices.Contracts;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class DashboardController(ICompanyService _companyService) : Controller
    {

        //Get all companies - Owned and Joined
        public async Task<IActionResult> Index()
        {
            string? currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(currentUserEmail);

            return View(model);
        }


        //Get request when trying to join a company
        [HttpGet("Dashboard/Join/{inviteCode?}")]
        public async Task<IActionResult> Join(string? inviteCode)
        {
            if (inviteCode == null)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Guid inviteGuid = Guid.Empty;
            if (Guid.TryParse(inviteCode, out inviteGuid) == false)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var model = await _companyService.GetCompanyFromInviteLinkAsync(inviteGuid);

            return View(model);
        }
    }
}
