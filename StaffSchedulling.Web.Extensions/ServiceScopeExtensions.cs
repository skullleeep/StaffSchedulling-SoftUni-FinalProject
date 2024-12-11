using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StaffScheduling.Common.Enums;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
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

            await roleManager.CreateDefaultRolesAsync();
        }

        public static async Task CreateDefaultAdminAsync(this IServiceScope scope)
        {
            var userManager =
                scope.ServiceProvider.GetRequiredService<ApplicationUserManager>();

            string name = "Admin";
            string email = DefaultAdminEmail;
            string password = DefaultAdminPassword;

            var usersInAdminRole = await userManager.GetUsersInRoleAsync(UserRole.Administrator.ToString());

            //If there are no admins then create one
            if (usersInAdminRole.Count == 0)
            {
                await userManager.CreateWithRoleAsync(email, password, name, UserRole.Administrator);
            }
        }
    }
}
