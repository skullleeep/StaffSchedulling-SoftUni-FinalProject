using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.ViewModels.Department;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Enums.CustomRoles;

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

        [Required]
        public PermissionRole CurrentEmployeePermission { get; set; }
    }
}
