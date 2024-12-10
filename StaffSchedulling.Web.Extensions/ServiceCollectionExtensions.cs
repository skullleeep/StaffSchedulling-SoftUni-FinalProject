using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository;
using StaffScheduling.Data.Repository.Contracts;
using StaffScheduling.Data.UnitOfWork;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Jobs;
using StaffScheduling.Web.Services.DbServices;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services
                //Company
                .AddScoped<IGenericRepository<Company, Guid>,
                            GenericRepository<Company, Guid>>()
                //Department
                .AddScoped<IGenericRepository<Department, Guid>,
                            GenericRepository<Department, Guid>>()
                //EmployeeInfo
                .AddScoped<IGenericRepository<EmployeeInfo, Guid>,
                            GenericRepository<EmployeeInfo, Guid>>()
                //Vacation
                .AddScoped<IGenericRepository<Vacation, Guid>,
                            GenericRepository<Vacation, Guid>>();
        }

        public static void RegisterUnitOfWork(this IServiceCollection services)
        {
            services
                //UnitOfWork
                .AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services
                //Permission Service
                .AddScoped<IPermissionService, PermissionService>()
                //Company Service
                .AddScoped<ICompanyService, CompanyService>()
                //EmployeeInfo Service
                .AddScoped<IEmployeeInfoService, EmployeeInfoService>()
                //Department Service
                .AddScoped<IDepartmentService, DepartmentService>()
                //Vacation Service
                .AddScoped<IVacationService, VacationService>();
        }

        public static void RegisterQuartzJobs(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                //q.UseMicrosoftDependencyInjectionJobFactory();

                //Register the cleanup job
                var jobKey = new JobKey(nameof(VacationCleanupJob));
                q.AddJob<VacationCleanupJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("VacationCleanupTrigger")
                    .WithCronSchedule("0 1 0 * * ?", x => x.InTimeZone(TimeZoneInfo.Local))); //Every day at 12:01 AM
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}
