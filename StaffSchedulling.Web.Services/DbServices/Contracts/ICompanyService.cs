using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Models.ViewModels.Company;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface ICompanyService
    {
        public Task<StatusReport> CreateCompanyAsync(CompanyCreateInputModel model, string userId);

        public Task<StatusReport> EditCompanyAsync(CompanyEditInputModel model, string userId);

        public Task<StatusReport> DeleteCompanyAsync(DeleteCompanyInputModel model);

        public Task<CompanyJoinViewModel?> GetCompanyFromInviteLinkAsync(Guid invite);

        public Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email, CompanySortFilter? sortFilter);

        public Task<CompanyAdministrationDashboardViewModel> GetCompaniesForAdministratorModel(string? searchQuery, CompanySortFilter? sortFilter, int page);

        public Task<CompanyManageViewModel?> GetManageCompanyModel(Guid id, PermissionRole userPermissionRole);

        public Task<CompanyEditInputModel?> GetCompanyEditInputModelAsync(Guid id);
    }
}
