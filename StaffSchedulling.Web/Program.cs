using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Extensions;
using StaffScheduling.Web.Services.UserServices;

namespace StaffScheduling.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			// Add services to the container.
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddUserManager<ApplicationUserManager>();

			//Custom extensions
			builder.Services.RegisterRepositories();
			builder.Services.RegisterServices();

			builder.Services.AddControllersWithViews();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapRazorPages();

			//Add custom roles
			using (var scope = app.Services.CreateScope())
			{
				await scope.CreateDefaultRolesAsync();
			}

			//Add custom admin
			/*            using (var scope = app.Services.CreateScope())
                        {
                            var userManager =
                                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                            string name = "Admin";
                            string email = DefaultAdminEmail;
                            string password = DefaultAdminPassword;

                            var usersInAdminRole = await userManager.GetUsersInRoleAsync(UserRole.Admin.ToString());

                            //If there are no admins then create one
                            if (usersInAdminRole.Count == 0)
                            {
                                await userManager.CreateUserAsync(email, password, name, UserRole.Admin);
                            }
                        }*/

			app.Run();
		}
	}
}
