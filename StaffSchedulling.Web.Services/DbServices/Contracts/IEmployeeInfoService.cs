using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Web.Models.ViewModels.EmployeeInfo;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string userId, string userEmail);

        public Task<PermissionRole> GetUserPermissionInCompanyAsync(Guid companyId, string userEmail);

        public Task<ManageEmployeesInfoViewModel?> GetCompanyManageEmployeeInfoModel(Guid companyId, string? searchQuery, EmployeeSearchFilter? searchFilter, int page = 1, int pageSize = 10);
    }
}
