using StaffScheduling.Common;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface ICompanyService
    {
        public Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId);

        public Task<StatusReport> EditCompanyAsync(CompanyEditInputModel model, string userId);

        public Task<StatusReport> DeleteCompanyAsync(Guid id);

        public Task<CompanyJoinViewModel?> GetCompanyFromInviteLinkAsync(Guid invite);

        public Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email);

        public Task<CompanyManageViewModel?> GetCompanyFromIdAsync(Guid id, bool UserCanEdit, bool UserCanDelete);

        public Task<CompanyEditInputModel?> GetCompanyEditInputModelAsync(Guid id);
    }
}
