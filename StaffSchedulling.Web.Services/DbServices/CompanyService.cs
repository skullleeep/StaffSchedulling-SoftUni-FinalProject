using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.ServiceErrorMessages;
using static StaffScheduling.Common.ServiceErrorMessages.CompanyService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class CompanyService(ApplicationDbContext _dbContext, ApplicationUserManager _userManager) : ICompanyService
    {
        public async Task<StatusReport> AddCompanyAsync(CompanyDto model)
        {
            var newEntity = new Company()
            {
                Name = model.Name,
                MaxVacationDaysPerYear = model.MaxVacationDaysPerYear,
                OwnerId = model.OwnerId,
            };

            try
            {
                await _dbContext.Companies.AddAsync(newEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport() { Ok = false, Message = $"Database Error: {ex.Message}" };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId)
        {
            var entityFound = await _dbContext
                .Companies
                .FirstOrDefaultAsync(c => c.OwnerId == userId && c.Name == model.Name);

            //Check if user already has a company with same name
            if (entityFound != null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CanNotCreateCompanyWithSameNameFormat, model.Name) };
            }

            var newEntity = new Company()
            {
                Name = model.Name,
                MaxVacationDaysPerYear = model.MaxVacationDaysPerYear,
                OwnerId = userId
            };

            try
            {
                await _dbContext.Companies.AddAsync(newEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport() { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<CompanyViewModel?> GetCompanyFromInviteLinkAsync(Guid invite)
        {
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Invite == invite);
            if (entity == null)
            {
                return null;
            }

            return new CompanyViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Invite = entity.Invite
            };
        }

        public async Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email)
        {
            var ownedCompanies = new List<CompanyDashboardViewModel>();

            var ownedCompanyIds = await _userManager.GetOwnedCompanyIdsFromUserEmailAsync(email);
            if (ownedCompanyIds != null)
            {
                ownedCompanies = await _dbContext
                                            .Companies
                                            .Where(c => ownedCompanyIds.Contains(c.Id))
                                            .Include(c => c.CompanyEmployeesInfo)
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = true
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();
            }

            var joinedCompanies = new List<CompanyDashboardViewModel>();

            var joinedCompanyIds = await _userManager.GetJoinedCompanyIdsFromUserEmailAsync(email);
            if (joinedCompanyIds != null)
            {
                joinedCompanies = await _dbContext
                                            .Companies
                                            .Where(c => joinedCompanyIds.Contains(c.Id))
                                            .Include(c => c.CompanyEmployeesInfo)
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = c.CompanyEmployeesInfo.Where(ef => ef.Email == email)
                                                                    .Select(ef => ef.Role)
                                                                    .Any(role => role == EmployeeRole.Admin || role == EmployeeRole.Supervisor),
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();
            }

            return new DashboardCompaniesViewModel
            {
                OwnedCompanies = ownedCompanies,
                JoinedCompanies = joinedCompanies
            };
        }

        public async Task<string> GetCompanyOwnerEmailFromIdAsync(Guid id)
        {
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
            {
                return String.Empty;
            }

            string ownerEmail = await _userManager.GetUserEmailFromIdAsync(entity.OwnerId);

            return ownerEmail;
        }

        public async Task<bool> HasCompanyWithIdAsync(Guid id)
        {
            //Check if company with this GUID exists
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                return false;
            }

            return true;
        }
    }
}
