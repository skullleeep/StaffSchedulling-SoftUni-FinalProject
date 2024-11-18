using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ServiceErrorMessages;
using static StaffScheduling.Common.ServiceErrorMessages.EmployeeInfoService;

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
    }
}
