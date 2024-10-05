namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class DashboardCompaniesViewModel
    {
        public ICollection<CompanyViewModel> OwnedCompanies { get; set; } = new List<CompanyViewModel>();

        public ICollection<CompanyViewModel> JoinedCompanies { get; set; } = new List<CompanyViewModel>();
    }
}
