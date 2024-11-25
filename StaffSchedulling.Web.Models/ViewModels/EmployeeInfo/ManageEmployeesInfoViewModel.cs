using StaffScheduling.Common.Enums;
using StaffScheduling.Web.Models.ViewModels.Department;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.EmployeeInfo
{
    public class ManageEmployeesInfoViewModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public List<EmployeeInfoViewModel> Employees { get; set; } = new List<EmployeeInfoViewModel>();

        [Required]
        public List<ManageEmployeesInfoDepartmentViewModel> Departments { get; set; } = new List<ManageEmployeesInfoDepartmentViewModel>();

        public string? SearchQuery { get; set; }

        public EmployeeSearchFilter? SearchFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;

        [Required]
        public int TotalPages { get; set; }
    }
}
