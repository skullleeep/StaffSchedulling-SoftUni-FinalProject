using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    public abstract class BaseController() : Controller
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        protected string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email)!;
        }

        protected bool IsGuidValid(string? id, ref Guid parsedGuid)
        {
            //Check for non-valid string
            if (String.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            //Check for invalid guid
            bool isGuidValid = Guid.TryParse(id, out parsedGuid);
            if (!isGuidValid)
            {
                return false;
            }

            return true;
        }
    }
}
