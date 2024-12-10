using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.InputModels.Vacation;
using StaffScheduling.Web.Models.ViewModels.Vacation;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IVacationService
    {
        public Task<StatusReport> AddVacationOfEmployeeAsync(AddVacationOfEmployeeInputModel model, string userId, bool isUnitTesting = false);

        public Task<StatusReport> DeleteVacationOfEmployeeAsync(DeleteVacationOfEmployeeInputModel model, string userId);

        public Task<StatusReport> DeleteAllVacationsOfEmployeeAsync(DeleteAllVacationsOfEmployeeInputModel model, string userId);

        public Task<StatusReport> ChangeStatusAsync(ChangeStatusInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId);

        public Task<StatusReport> DeleteVacationOfCompanyAsync(DeleteVacationOfCompanyInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId);

        public Task<StatusReport> DeleteAllVacationsOfCompanyAsync(DeleteAllVacationsOfCompanyInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId);

        public Task<ManageScheduleViewModel?> GetCompanyManageScheduleModelAsync(Guid companyId, VacationSortFilter? sortFilter, int page, string userId);

        public Task<ManageVacationsViewModel?> GetCompanyManageVacationsModelAsync(Guid companyId, string? searchQuery, VacationSortFilter? sortFilter, int page, PermissionRole userPermissionRole, Guid? userNeededDepartmentId);
    }
}
