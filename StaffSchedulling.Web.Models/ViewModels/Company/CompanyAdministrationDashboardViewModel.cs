using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyAdministrationDashboardViewModel : CompanyAdministrationDashboardFilters
    {
        [Required]
        public List<CompanyAdministrationViewModel> Companies { get; set; } = new List<CompanyAdministrationViewModel>();

        [Required]
        public int TotalPages { get; set; }
    }
}
