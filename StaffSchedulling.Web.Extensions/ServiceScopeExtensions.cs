using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
