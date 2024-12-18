﻿using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
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
                .FirstOrDefaultAsync(c => c.OwnerId == userId && c.NormalizedName == model.Name.ToUpper());

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
                return new StatusReport { Ok = false, Message = String.Format(CreatedCompaniesLimitHitFormat, UserCreatedCompaniesLimit) };
            }

            var newEntity = new Company()
            {
                Name = model.Name,
                NormalizedName = model.Name.ToUpper(),
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
                .FirstOrDefaultAsync(c => c.Id != model.Id && c.OwnerId == userId && c.NormalizedName == model.Name.ToUpper());

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
                entity.NormalizedName = model.Name.ToUpper();
                entity.MaxVacationDaysPerYear = model.MaxVacationDaysPerYear;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteCompanyAsync(DeleteCompanyInputModel model)
        {
            var entityCompany = await _unitOfWork
                .Companies
                .All()
                .Include(c => c.CompanyEmployeesInfo)
                .Include(c => c.Departments)
                .Include(c => c.Vacations)
                .FirstOrDefaultAsync(c => c.Id == model.CompanyId);

            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            try
            {
                _unitOfWork.Departments.DeleteRange(entityCompany.Departments.ToArray());

                _unitOfWork.Vacations.DeleteRange(entityCompany.Vacations.ToArray());

                _unitOfWork.EmployeesInfo.DeleteRange(entityCompany.CompanyEmployeesInfo.ToArray());

                _unitOfWork.Companies.Delete(entityCompany);

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

        public async Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email, CompanySortFilter? sortFilter)
        {
            //Order by NameAscending by default
            if (sortFilter.HasValue == false)
            {
                sortFilter = CompanySortFilter.NameAscending;
            }

            //Get EmployeeRole roles which have >= PermissionRole that is needed to manage
            //Doing this because RoleMapping[EmployeeRole] can't be translated into SQL from entity
            List<EmployeeRole> rolesWithAccess = GetRolesWithAccess(PermissionRole.Manager);

            var ownedCompanyIds = await _userManager.GetOwnedCompanyIdsFromUserEmailAsync(email);
            var joinedCompanyIds = await _userManager.GetJoinedCompanyIdsFromUserEmailAsync(email);

            IQueryable<CompanyDashboardViewModel> selectedOwnedCompanies = _unitOfWork
                                            .Companies
                                            .All()
                                            .Include(c => c.CompanyEmployeesInfo)
                                            .AsNoTracking()
                                            .Where(c => ownedCompanyIds.Contains(c.Id))
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = true
                                            });


            IQueryable<CompanyDashboardViewModel> selectedJoinedCompanies = _unitOfWork
                                            .Companies
                                            .All()
                                            .Include(c => c.CompanyEmployeesInfo)
                                            .AsNoTracking()
                                            .Where(c => joinedCompanyIds.Contains(c.Id))
                                            .Select(c => new CompanyDashboardViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite,
                                                UserCanManage = c.CompanyEmployeesInfo
                                                    .Where(ef => ef.Email == email)
                                                    .Select(ef => ef.Role)
                                                    .Any(role => rolesWithAccess.Contains(role)),
                                            });

            //Filter companies up to CompanySortFilter
            if (sortFilter.Value == CompanySortFilter.NameAscending)
            {
                selectedOwnedCompanies = selectedOwnedCompanies.OrderBy(c => c.Name);
                selectedJoinedCompanies = selectedJoinedCompanies.OrderBy(c => c.Name);
            }
            else if (sortFilter.Value == CompanySortFilter.NameDescending)
            {
                selectedOwnedCompanies = selectedOwnedCompanies.OrderByDescending(c => c.Name);
                selectedJoinedCompanies = selectedJoinedCompanies.OrderByDescending(c => c.Name);
            }


            List<CompanyDashboardViewModel> ownedCompanyModels = await selectedOwnedCompanies.ToListAsync();
            List<CompanyDashboardViewModel> joinedCompanyModels = await selectedJoinedCompanies.ToListAsync();

            return new DashboardCompaniesViewModel
            {
                OwnedCompanies = ownedCompanyModels,
                JoinedCompanies = joinedCompanyModels
            };
        }

        public async Task<CompanyAdministrationDashboardViewModel> GetCompaniesForAdministratorModel(string? searchQuery, CompanySortFilter? sortFilter, int page)
        {
            //Order by NameAscending by default
            if (sortFilter.HasValue == false)
            {
                sortFilter = CompanySortFilter.NameAscending;
            }

            //Get IQueryable of the most basic needed results
            IQueryable<Company> entitiesBase = _unitOfWork
                .Companies
                .All()
                .Include(c => c.Owner)
                .AsNoTracking();

            //Check if searchQuery has value then filter companies
            if (!string.IsNullOrEmpty(searchQuery))
            {
                entitiesBase = entitiesBase
                    .Where(c => c.NormalizedName.Contains(searchQuery.ToUpper())
                                || c.Owner.Email!.ToLower().Contains(searchQuery.ToLower()));
            }

            //Calculate total employees and pages
            int totalEmployees = await entitiesBase.CountAsync();
            int totalPages = (int)Math.Ceiling(totalEmployees / (double)DashboardAdministrationCompaniesPageSize);

            //Check if page is non-existent page
            //If it is then make page last page
            if (page > totalPages || page < 1)
            {
                page = Math.Max(totalPages, 1);
            }

            IQueryable<CompanyAdministrationViewModel> selectedModels = entitiesBase
                .Select(c => new CompanyAdministrationViewModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    OwnerEmail = c.Owner.Email!,
                    Invite = c.Invite,
                });


            if (sortFilter == CompanySortFilter.NameAscending)
            {
                selectedModels = selectedModels.OrderBy(c => c.Name);
            }
            else if (sortFilter == CompanySortFilter.NameDescending)
            {
                selectedModels = selectedModels.OrderByDescending(c => c.Name);
            }

            List<CompanyAdministrationViewModel> companyModels = await selectedModels
                .Skip((page - 1) * DashboardAdministrationCompaniesPageSize)
                .Take(DashboardAdministrationCompaniesPageSize)
                .ToListAsync();

            return new CompanyAdministrationDashboardViewModel()
            {
                Companies = companyModels,
                SearchQuery = searchQuery,
                SortFilter = sortFilter,
                CurrentPage = page,
                TotalPages = totalPages,
            };
        }

        public async Task<CompanyManageViewModel?> GetManageCompanyModel(Guid id, PermissionRole userPermissionRole)
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
                UserCanEdit = userPermissionRole >= PermissionRole.Editor,
                UserCanDelete = userPermissionRole >= PermissionRole.Owner,
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
