using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;

namespace StaffScheduling.Web.Services.DbServices
{
    public class CompanyService(ApplicationDbContext _dbContext, ApplicationUserManager _userManager, IEmployeeInfoService _employeeInfoService) : ICompanyService
    {
        public async Task<StatusReport> AddCompanyAsync(CompanyDto model)
        {
            var newEntity = new Company()
            {
                Name = model.Name,
                OwnerId = model.OwnerId,
            };

            try
            {
                await _dbContext.Companies.AddAsync(newEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return new StatusReport() { Ok = false, Message = "Could not add new Company" };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email)
        {
            var user = await _dbContext.ApplicationUsers
                .Include(u => u.CompaniesOwned)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            var ownedCompanies = user
                .CompaniesOwned
                .Select(c => new CompanyViewModel()
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            var joinedCompanyIds = await _employeeInfoService.GetJoinedCompanyIdsFromEmailAsync(email);
            var joinedCompanies = _dbContext
                .Companies
                .Where(c => joinedCompanyIds.Contains(c.Id))
                .Select(c => new CompanyViewModel()
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            return new DashboardCompaniesViewModel
            {
                OwnedCompanies = ownedCompanies,
                JoinedCompanies = joinedCompanies
            };
        }
    }
}
