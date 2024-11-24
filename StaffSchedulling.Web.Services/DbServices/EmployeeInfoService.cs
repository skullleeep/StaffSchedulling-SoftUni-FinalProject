using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.ViewModels.Department;
using StaffScheduling.Web.Models.ViewModels.EmployeeInfo;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.EmployeeInfoService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class EmployeeInfoService(IUnitOfWork _unitOfWork, ApplicationUserManager _userManager) : IEmployeeInfoService
    {
        public async Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string userId, string userEmail)
        {
            //Check if userId is valid
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new StatusReport { Ok = false, Message = CouldNotFindUser };
            }

            var entityCompany = await _unitOfWork
                            .Companies
                            .All()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company with id exists
            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            //Get company owner's email
            string companyOwnerEmail = await _userManager.GetUserEmailFromIdAsync(entityCompany.OwnerId);

            //Check if user trying to join is the company's owner
            if (companyOwnerEmail == userEmail)
            {
                return new StatusReport { Ok = false, Message = OwnerCouldNotHisJoinCompany };
            }

            int joinedCompaniesCount = await _unitOfWork
                .EmployeesInfo
                .All()
                .Where(e => e.Email == userEmail && e.HasJoined == true)
                .AsNoTracking()
                .CountAsync();

            //Check if user has hit joined companies limit
            if (joinedCompaniesCount >= UserJoinedCompaniesLimit)
            {
                return new StatusReport { Ok = false, Message = String.Format(JoinedCompaniesLimitHitFormat, joinedCompaniesCount) };
            }

            var employeeInfo = await _unitOfWork
                .EmployeesInfo
                .All()
                .Where(e => e.CompanyId == companyId && e.Email == userEmail)
                .FirstOrDefaultAsync();

            //Check for wrong employeeInfo
            if (employeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CouldNotFindEmployeeInfoFormat, userEmail) };
            }

            //Check if employee has already joined
            if (employeeInfo.HasJoined == true)
            {
                return new StatusReport { Ok = false, Message = CouldNotJoinAlreadyJoinedCompany };
            }

            try
            {
                employeeInfo.HasJoined = true;
                employeeInfo.UserId = userId;
                await _unitOfWork.SaveChangesAsync();

                return new StatusReport { Ok = true };
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }
        }

        public async Task<PermissionRole> GetUserPermissionInCompanyAsync(Guid companyId, string userEmail)
        {
            var entityCompany = await _unitOfWork
                .Companies
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company with id exists
            if (entityCompany == null)
            {
                return PermissionRole.None;
            }

            string companyOwnerEmail = await _userManager.GetUserEmailFromIdAsync(entityCompany.OwnerId);

            if (userEmail == companyOwnerEmail)
            {
                return PermissionRole.Owner;
            }

            var entity = await _unitOfWork
                .EmployeesInfo
                .All()
                .Where(e => e.HasJoined == true && e.Email == userEmail && e.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return PermissionRole.None;
            }

            return RoleMapping[entity.Role];
        }

        public async Task<ManageEmployeesInfoViewModel?> GetCompanyManageEmployeeInfoModel(Guid companyId, string searchQuery, SearchFilter searchFilter = SearchFilter.Email, int page = 1, int pageSize = 10)
        {
            var entityCompany = await _unitOfWork
                .Companies
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company exists
            if (entityCompany == null)
            {
                return null;
            }


            //Get IQueryable of the most basic needed results
            IQueryable<EmployeeInfo> selectedEmployeesInfo = _unitOfWork
                .EmployeesInfo
                .All()
                .Include(ef => ef.Department)
                .Include(ef => ef.User)
                .Where(ef => ef.CompanyId == companyId);

            //If searchQuery has something then filter employees up to SearchFilter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchFilter == SearchFilter.Email)
                {
                    selectedEmployeesInfo = selectedEmployeesInfo
                        .Where(ef => ef.Email.ToLower().Contains(searchQuery.ToLower()));
                }
                else if (searchFilter == SearchFilter.Name)
                {
                    selectedEmployeesInfo = selectedEmployeesInfo
                        .Where(ef => ef.User != null)
                        .Where(ef => ef.User!.FullName!.ToLower().Contains(searchQuery.ToLower()));
                }
            }

            //Calculate total records and pages
            int totalEmployees = selectedEmployeesInfo.Count();
            int totalPages = (int)Math.Ceiling(totalEmployees / (double)pageSize);

            List<EmployeeInfoViewModel> employeesInfo = await selectedEmployeesInfo
                .OrderBy(e => e.Email)
                .Select(ef => new EmployeeInfoViewModel()
                {
                    Id = ef.Id,
                    Name = ef.User == null ? null : ef.User.FullName,
                    Email = ef.Email,
                    Department = ef.Department == null ? null : ef.Department.Name,
                    HasJoined = ef.HasJoined,
                    Role = ef.Role
                })
                .ToListAsync();

            //Get company departments
            List<ManageEmployeesInfoDepartmentViewModel> departments = await _unitOfWork
                .Departments
                .All()
                .Where(d => d.CompanyId == companyId)
                .Select(d => new ManageEmployeesInfoDepartmentViewModel()
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .AsNoTracking()
                .ToListAsync();

            return new ManageEmployeesInfoViewModel()
            {
                CompanyId = companyId,
                SearchQuery = searchQuery,
                SearchFilter = searchFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                Employees = employeesInfo,
                Departments = departments
            };
        }
    }
}
