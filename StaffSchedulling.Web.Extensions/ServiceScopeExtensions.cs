using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StaffScheduling.Common.Enums;

namespace StaffScheduling.Web.Extensions
{
	public static class ServiceScopeExtensions
	{

		public static async Task CreateDefaultRolesAsync(this IServiceScope scope)
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
