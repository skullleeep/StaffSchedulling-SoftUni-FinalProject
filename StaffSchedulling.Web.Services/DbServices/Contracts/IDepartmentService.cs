using StaffScheduling.Common;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IDepartmentService
    {
        public Task<StatusReport> DeleteDepartmentsWithCompanyIdAsync(Guid companyId);
    }
}
