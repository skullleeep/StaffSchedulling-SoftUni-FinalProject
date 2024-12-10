using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Department
{
    public class ManageDepartmentsViewModel : ManageDepartmentsFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required]
        public List<DepartmentManageViewModel> Departments { get; set; } = new List<DepartmentManageViewModel>();

        [Required]
        public int TotalPages { get; set; }
    }
}
