using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StaffScheduling.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
