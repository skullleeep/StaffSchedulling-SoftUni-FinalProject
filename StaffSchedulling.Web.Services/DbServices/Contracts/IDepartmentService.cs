using StaffScheduling.Common;
using StaffScheduling.Web.Models.InputModels.Department;
using StaffScheduling.Web.Models.ViewModels.Department;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IDepartmentService
    {
        public Task<StatusReport> AddDepartmentManuallyAsync(AddDepartmentManuallyInputModel model);

        public Task<StatusReport> DeleteDepartmentAsync(DeleteDepartmentInputModel model);

        public Task<StatusReport> DeleteAllDepartmentsAsync(DeleteAllDepartmentsInputModel model);

        public Task<ManageDepartmentsViewModel?> GetCompanyManageDepartmentsModel(Guid companyId, int page);
    }
}
