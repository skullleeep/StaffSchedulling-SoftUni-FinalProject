using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.ServiceErrorMessages.EmployeeInfoService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class EmployeeInfoService(ApplicationDbContext _dbContext, ApplicationUserManager _userManager) : IEmployeeInfoService
    {
        public async Task<StatusReport> JoinCompanyWithIdAsync(int companyId, string companyOwnerEmail, string userId)
        {

            var userEmail = await _userManager.GetUserEmailFromIdAsync(userId);
            if (String.IsNullOrEmpty(userEmail))
            {
                return new StatusReport { Ok = false, Message = CouldNotFindUserEmail };
            }

            if (await _userManager.HasUserWithEmailAsync(userEmail) == false)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindUser };
            }

            if (companyOwnerEmail == await _userManager.GetUserEmailFromIdAsync(userId))
            {
                return new StatusReport { Ok = false, Message = OwnerCouldNotHisJoinCompany };
            }

            var employeeInfo = await _dbContext
                .EmployeesInfo
                .Where(e => e.Email == userEmail)
                .Where(e => e.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (employeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CouldNotFindEmployeeInfoFormat, userEmail) };
            }

            if (employeeInfo.HasJoined == true)
            {
                return new StatusReport { Ok = false, Message = CouldNotJoinAlreadyJoinedCompany };
            }

            try
            {
                employeeInfo.HasJoined = true;
                employeeInfo.UserId = userId;
                await _dbContext.SaveChangesAsync();

                return new StatusReport { Ok = true };
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }
        }
    }
}
