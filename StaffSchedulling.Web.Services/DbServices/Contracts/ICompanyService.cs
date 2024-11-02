using StaffScheduling.Common;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface ICompanyService
    {
        public Task<StatusReport> AddCompanyAsync(CompanyDto model);

        public Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId);

        public Task<CompanyViewModel?> GetCompanyFromInviteLinkAsync(Guid invite);

        public Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email);

        public Task<string> GetCompanyOwnerEmailFromIdAsync(Guid id);

        public Task<bool> HasCompanyWithIdAsync(Guid id);
    }
}
