using StaffScheduling.Common;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IVacationService
    {
        public Task<StatusReport> DeleteVacationsWithCompanyIdAsync(Guid companyId);
    }
}
