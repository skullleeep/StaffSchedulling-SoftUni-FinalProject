using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StaffScheduling.Common.Enums;
using static StaffScheduling.Common.ErrorMessages.ApplicationErrorMessages;

namespace StaffScheduling.Web.Extensions
{
    public static class ServiceScopeExtensions
    {

        public static async Task CreateDefaultRolesAsync(this IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Check if RoleManager was not found
            if (roleManager == null)
            {
                throw new ArgumentNullException(nameof(roleManager), String.Format(CouldNotFindServiceFormat, nameof(roleManager)));
            }

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
