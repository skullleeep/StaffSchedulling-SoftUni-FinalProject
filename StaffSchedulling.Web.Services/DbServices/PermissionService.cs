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
    }

}
