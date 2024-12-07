using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.InputModels.Vacation;
using StaffScheduling.Web.Models.ViewModels.Vacation;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IVacationService
    {
        public Task<StatusReport> AddVacationOfEmployeeAsync(AddVacationOfEmployeeInputModel model, string userId);

        public Task<StatusReport> DeleteEmployeeOfEmployeeAsync(DeleteVacationOfEmployeeInputModel model, string userId);

        public Task<StatusReport> DeleteAllVacationsOfEmployeeAsync(DeleteAllVacationsOfEmployeeInputModel model, string userId);

        public Task<ManageScheduleViewModel?> GetCompanyManageScheduleModel(Guid companyId, VacationSortFilter? sortFilter, int page, string userId);
    }
}
