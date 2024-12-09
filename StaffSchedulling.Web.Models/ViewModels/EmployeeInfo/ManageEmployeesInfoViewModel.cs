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
        public string CompanyName { get; set; } = null!;

        [Required]
        public List<EmployeeInfoViewModel> Employees { get; set; } = new List<EmployeeInfoViewModel>();

        [Required]
        public PermissionRole CurrentUserPermission { get; set; } = PermissionRole.None;

        [Required]
        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        public string? SearchQuery { get; set; }

        public EmployeeSearchFilter? SearchFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;

        [Required]
        public int TotalPages { get; set; }
    }
}
