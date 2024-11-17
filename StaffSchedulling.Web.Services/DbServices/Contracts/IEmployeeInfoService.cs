using StaffScheduling.Common;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string companyOwnerEmail, string userId);

        public Task<PermissionRole> GetRoleOfEmployeeInCompanyAsync(Guid companyId, string companyOwnerEmail, string userEmail);
    }
}
