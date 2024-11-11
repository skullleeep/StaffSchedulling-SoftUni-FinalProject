using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.ServiceErrorMessages;
using static StaffScheduling.Common.ServiceErrorMessages.EmployeeInfoService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class EmployeeInfoService(IGuidRepository<EmployeeInfo> _employeeInfoRepo, ApplicationUserManager _userManager) : IEmployeeInfoService
    {
        public async Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string companyOwnerEmail, string userId)
        {

            var userEmail = await _userManager.GetUserEmailFromIdAsync(userId);

            //Check for wrong userId
            if (String.IsNullOrEmpty(userEmail))
            {
                return new StatusReport { Ok = false, Message = CouldNotFindUserEmail };
            }

            //Check if user trying to join is the company's owner
            if (companyOwnerEmail == userEmail)
            {
                return new StatusReport { Ok = false, Message = OwnerCouldNotHisJoinCompany };
            }

            var employeeInfo = await _employeeInfoRepo
                .All()
                .Where(e => e.Email == userEmail)
                .Where(e => e.CompanyId == companyId)
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
                await _employeeInfoRepo.SaveAsync();

                return new StatusReport { Ok = true };
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }
        }
    }
}
