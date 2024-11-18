using StaffScheduling.Common;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string userId, string userEmail);

        public Task<PermissionRole> GetUserPermissionInCompanyAsync(Guid companyId, string userEmail);
    }
}
