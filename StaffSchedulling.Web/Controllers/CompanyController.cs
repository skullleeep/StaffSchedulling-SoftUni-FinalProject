using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class CompanyController(ICompanyService _companyService) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

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

            return View(model);
        }
    }
}
