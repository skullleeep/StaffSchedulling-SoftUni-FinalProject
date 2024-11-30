using Microsoft.AspNetCore.Identity;
using StaffScheduling.Common.Enums;

namespace StaffScheduling.Web.Extensions
{
    public static class RoleManagerExtensions
    {
        public static async Task CreateDefaultRolesAsync(this RoleManager<IdentityRole> roleManager)
        {
            //Get roles from enum
            string[] roles = Enum.GetNames(typeof(UserRole));

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
