using StaffScheduling.Common;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<StatusReport> JoinCompanyWithIdAsync(int companyId, string companyOwnerEmail, string userId);
    }
}
