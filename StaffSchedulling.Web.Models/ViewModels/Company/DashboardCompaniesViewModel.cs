using StaffScheduling.Common.Enums.Filters;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class DashboardCompaniesViewModel
    {
        public ICollection<CompanyDashboardViewModel> OwnedCompanies { get; set; } = new List<CompanyDashboardViewModel>();

        public ICollection<CompanyDashboardViewModel> JoinedCompanies { get; set; } = new List<CompanyDashboardViewModel>();

        public CompanySortFilter? SortFilter { get; set; }
    }
}
