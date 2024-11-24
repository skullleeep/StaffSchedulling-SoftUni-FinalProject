using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using System.Data;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.CompanyService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class CompanyService(IUnitOfWork _unitOfWork, ApplicationUserManager _userManager) : ICompanyService
    {
        public async Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId)
        {
            var entityFound = await _unitOfWork
                .Companies
                .FirstOrDefaultAsync(c => c.OwnerId == userId && c.Name == model.Name);

            //Check if user already has a company with same name
            if (entityFound != null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CanNotCreateCompanyWithSameNameFormat, model.Name) };
            }

            int createdCompaniesCount = await _unitOfWork
                .Companies
                .All()
                .Where(c => c.OwnerId == userId)
                .AsNoTracking()
                .CountAsync();

            //Check if user has hit the created companies limit
            if (createdCompaniesCount >= UserCreatedCompaniesLimit)
            {
                return new StatusReport { Ok = false, Message = String.Format(CreatedCompaniesLimitHitFormat, createdCompaniesCount) };
            }

            var newEntity = new Company()
            {
                Name = model.Name,
                MaxVacationDaysPerYear = model.MaxVacationDaysPerYear,
                OwnerId = userId
            };

            try
            {
                await _unitOfWork.Companies.AddAsync(newEntity);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport() { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<StatusReport> EditCompanyAsync(CompanyEditInputModel model, string userId)
        {
            var entityFound = await _unitOfWork
                .Companies
                .FirstOrDefaultAsync(c => c.OwnerId == userId && c.Name == model.Name);

            //Check if user already has a company with same name
            if (entityFound != null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CanNotEditCompanyWithSameNameFormat, model.Name) };
            }

            var entity = await _unitOfWork
                .Companies
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            //Check if company with id exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            try
            {
                entity.Name = model.Name;
                entity.MaxVacationDaysPerYear = model.MaxVacationDaysPerYear;

                _unitOfWork
                    .Companies
                    .Update(entity);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteCompanyAsync(Guid id)
        {
            var entityCompany = await _unitOfWork
                .Companies
                .All()
                .Include(c => c.CompanyEmployeesInfo)
                .Include(c => c.Departments)
                .Include(c => c.Vacations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            try
            {
                _unitOfWork.Departments.DeleteRange(entityCompany.Departments.ToArray());

                _unitOfWork.Vacations.DeleteRange(entityCompany.Vacations.ToArray());

                _unitOfWork.EmployeesInfo.DeleteRange(entityCompany.CompanyEmployeesInfo.ToArray());

                await _unitOfWork.Companies.DeleteAsync(id);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport() { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<CompanyJoinViewModel?> GetCompanyFromInviteLinkAsync(Guid invite)
        {
            var entity = await _unitOfWork
                .Companies
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
                ownedCompanies = await _unitOfWork
                                            .Companies
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
                //Get EmployeeRole roles which can have >= PermissionRole that is needed to manage
                //Doing this because RoleMapping[EmployeeRole] can't be translated into SQL from entity
                List<EmployeeRole> rolesWithAccess = RoleMapping
                    .Where(rm => rm.Value >= PermissionRole.Manager)
                    .Select(rm => rm.Key)
                    .ToList();

                joinedCompanies = await _unitOfWork
                                            .Companies
                                            .All()
                                            .Where(c => joinedCompanyIds.Contains(c.Id))
                                            .Include(c => c.CompanyEmployeesInfo)
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = c.CompanyEmployeesInfo
                                                                    .Where(ef => ef.Email == email)
                                                                    .Select(ef => ef.Role)
                                                                    .Any(role => rolesWithAccess.Contains(role)),
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

        public async Task<CompanyManageViewModel?> GetCompanyFromIdAsync(Guid id, bool UserCanEdit, bool UserCanDelete)
        {
            var entity = await _unitOfWork
                .Companies
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
                Invite = entity.Invite,
                MaxVacationDaysPerYear = entity.MaxVacationDaysPerYear,
                UserCanEdit = UserCanEdit,
                UserCanDelete = UserCanDelete
            };
        }

        public async Task<CompanyEditInputModel?> GetCompanyEditInputModelAsync(Guid id)
        {
            var entity = await _unitOfWork
                .Companies
                .All()
                .Where(c => c.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }

            return new CompanyEditInputModel
            {
                Id = entity.Id,
                Name = entity.Name,
                MaxVacationDaysPerYear = entity.MaxVacationDaysPerYear
            };
        }
    }
}
