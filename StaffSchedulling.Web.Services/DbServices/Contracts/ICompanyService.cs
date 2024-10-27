using StaffScheduling.Common;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.ViewModels.Company;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface ICompanyService
    {
        public Task<StatusReport> AddCompanyAsync(CompanyDto model);

        public Task<CompanyViewModel?> GetCompanyFromInviteLinkAsync(Guid invite);

        public Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string emailId);

        public Task<string> GetCompanyOwnerEmailFromIdAsync(int id);

        public Task<bool> HasCompanyWithIdAsync(int id);
    }
}
