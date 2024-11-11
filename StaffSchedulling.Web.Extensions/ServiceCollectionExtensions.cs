using Microsoft.Extensions.DependencyInjection;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository;
using StaffScheduling.Data.Repository.Contracts;
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
                .AddScoped<IGuidRepository<Company>,
                            GuidRepository<Company>>()
                //Department
                .AddScoped<IGuidRepository<Department>,
                            GuidRepository<Department>>()
                //EmployeeInfo
                .AddScoped<IGuidRepository<EmployeeInfo>,
                            GuidRepository<EmployeeInfo>>()
                //Vacation
                .AddScoped<IGuidRepository<Vacation>,
                            GuidRepository<Vacation>>();
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services
                //Company Service
                .AddScoped<ICompanyService, CompanyService>()
                //EmployeeInfo Service
                .AddScoped<IEmployeeInfoService, EmployeeInfoService>();
        }
    }
}
