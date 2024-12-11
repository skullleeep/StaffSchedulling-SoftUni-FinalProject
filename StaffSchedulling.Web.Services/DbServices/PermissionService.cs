using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices
{
    public class PermissionService(IUnitOfWork _unitOfWork, ApplicationUserManager _userManager) : IPermissionService
    {
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

            //Check if user is in role 'Administrator'
            //And if it is give full-access rights
            if (await _userManager.IsUserAdministratorFromEmailAsync(userEmail))
            {
                return PermissionRole.Administrator;
            }

            var entityEmployeeInfo = await _unitOfWork
                .EmployeesInfo
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.HasJoined == true && e.Email == userEmail && e.CompanyId == companyId);

            if (entityEmployeeInfo == null)
            {
                return PermissionRole.None;
            }

            return RoleMapping[entityEmployeeInfo.Role];
        }

        public async Task<Guid?> GetUserNeededDepartmentId(Guid companyId, string userEmail)
        {
            //Get roles which need to have a department
            //Used to check if user as an employee of a company is in one of those roles
            var rolesWhichNeedDepartment = GetRolesWhichNeedDepartment();

            var entityCompany = await _unitOfWork
                            .Companies
                            .All()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company with id exists
            if (entityCompany == null)
            {
                return null;
            }

            var entityEmployeeInfo = await _unitOfWork
                .EmployeesInfo
                .All()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.HasJoined == true
                        && e.Email == userEmail
                        && e.CompanyId == companyId
                        && rolesWhichNeedDepartment.Contains(e.Role)
                        && e.DepartmentId != null);

            if (entityEmployeeInfo == null)
            {
                return null;
            }

            return entityEmployeeInfo.DepartmentId!.Value;
        }
    }

}
