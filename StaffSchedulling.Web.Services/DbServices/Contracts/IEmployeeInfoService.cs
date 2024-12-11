using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Models.ViewModels.EmployeeInfo;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string userId, string userEmail);

        public Task<StatusReport> AddEmployeeManuallyAsync(AddEmployeeInfoManuallyInputModel model);

        public Task<StatusReport> DeleteEmployeeAsync(DeleteEmployeeInputModel model, PermissionRole userPermissionRole);

        public Task<StatusReport> DeleteAllEmployeesAsync(DeleteAllEmployeesInputModel model, PermissionRole userPermissionRole);

        public Task<StatusReport> ChangeRoleAsync(ChangeRoleInputModel model, PermissionRole userPermissionRole);

        public Task<StatusReport> ChangeDepartmentAsync(ChangeDepartmentInputModel model, PermissionRole userPermissionRole);

        public Task<StatusReport> ChangeEmailOfUser(string oldEmail, string newEmail);

        public Task<ManageEmployeesInfoViewModel?> GetCompanyManageEmployeeInfoModel(Guid companyId, string? searchQuery, EmployeeSearchFilter? searchFilter, int page, PermissionRole userPermissionRole);
    }
}
