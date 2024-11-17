using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using System.Data;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ServiceErrorMessages;
using static StaffScheduling.Common.ServiceErrorMessages.CompanyService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class CompanyService(IGuidRepository<Company> _companyRepo, ApplicationUserManager _userManager) : ICompanyService
    {
        public async Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId)
        {
            var entityFound = await _companyRepo
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
                await _companyRepo.AddAsync(newEntity);
                await _companyRepo.SaveAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport() { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<CompanyJoinViewModel?> GetCompanyFromInviteLinkAsync(Guid invite)
        {
            var entity = await _companyRepo
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Invite == invite);
            if (entity == null)
            {
                return null;
            }

            return new CompanyJoinViewModel
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
                ownedCompanies = await _companyRepo
                                            .All()
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
                var companyData = await _companyRepo
                    .All()
                    .Where(c => joinedCompanyIds.Contains(c.Id))
                    .Include(c => c.CompanyEmployeesInfo)
                    .Select(c => new CompanyDashboardDto()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Invite = c.Invite,
                        Role = c.CompanyEmployeesInfo
                            .Where(ef => ef.Email == email)
                            .Select(ef => ef.Role)
                            .FirstOrDefault()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                joinedCompanies = companyData
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = RoleMapping[c.Role] >= PermissionRole.Editor,
                                            })
                                            .ToList();
            }

            return new DashboardCompaniesViewModel
            {
                OwnedCompanies = ownedCompanies,
                JoinedCompanies = joinedCompanies
            };
        }

        public async Task<CompanyManageViewModel?> GetCompanyFromIdAsync(Guid id, bool UserCanEdit, bool UserCanDelete)
        {
            var entity = await _companyRepo
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                return null;
            }

            return new CompanyManageViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                MaxVacationDaysPerYear = entity.MaxVacationDaysPerYear,
                UserCanEdit = UserCanEdit,
                UserCanDelete = UserCanDelete
            };
        }

        public async Task<string?> GetCompanyOwnerEmailFromIdAsync(Guid id)
        {
            var entity = await _companyRepo
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
            {
                return null;
            }

            string ownerEmail = await _userManager.GetUserEmailFromIdAsync(entity.OwnerId);

            return ownerEmail;
        }
    }
}
