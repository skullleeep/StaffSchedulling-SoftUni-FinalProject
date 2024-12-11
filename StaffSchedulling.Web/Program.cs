using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Seeding;
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

            //Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            //Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<ApplicationUserManager>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Home/Error/403"); //Redirect for access denied (403)
            });

            //Custom registrations
            builder.Services.RegisterRepositories();
            builder.Services.RegisterUnitOfWork();
            builder.Services.RegisterServices();
            builder.Services.RegisterQuartzJobs();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            var app = builder.Build();

            //Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                //The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //Security Setup
            app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.Enabled());
            app.UseXfo(options => options.Deny());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");

            app.MapControllerRoute(
                name: "Areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "Errors",
                pattern: "{controller=Home}/{action=Index}/{statusCode?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            //Add custom roles
            using (var scope = app.Services.CreateScope())
            {
                await scope.CreateDefaultRolesAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                //Seed data
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    await DataSeeder.SeedData(context, manager);
                }

                //Add custom admin
                using (var scope = app.Services.CreateScope())
                {
                    await scope.CreateDefaultAdminAsync();
                }
            }

            app.Run();
        }
    }
}
