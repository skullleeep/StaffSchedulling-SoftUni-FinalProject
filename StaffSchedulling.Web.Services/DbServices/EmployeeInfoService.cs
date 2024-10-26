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
        public async Task<List<int>> GetJoinedCompanyIdsFromEmailAsync(string email)
        {
            return await _dbContext.EmployeesInfo
                .Where(e => e.Email == email && e.HasJoined == true)
                .Select(e => e.CompanyId)
                .ToListAsync();
        }

        public async Task<StatusReport> JoinCompanyWithIdAsync(int companyId, string email)
        {

            if (await _userManager.HasUserWithEmailAsync(email) == false)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindUser };
            }

            var employeeInfo = await _dbContext
                .EmployeesInfo
                .Where(e => e.Email == email)
                .Where(e => e.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (employeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CouldNotFindEmployeeInfoFormat, email) };
            }

            try
            {
                employeeInfo.HasJoined = true;
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
