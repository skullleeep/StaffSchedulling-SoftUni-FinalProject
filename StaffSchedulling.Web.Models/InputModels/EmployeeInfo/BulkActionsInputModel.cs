using StaffScheduling.Web.Models.ViewModels.Department;
using StaffScheduling.Web.Models.ViewModels.EmployeeInfo;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class BulkActionsInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public List<EmployeeInfoViewModel> Employees { get; set; } = new List<EmployeeInfoViewModel>();

        [Required]
        public PermissionRole CurrentUserPermission { get; set; } = PermissionRole.None;

        [Required]
        public List<ManageEmployeesInfoDepartmentViewModel> Departments { get; set; } = new List<ManageEmployeesInfoDepartmentViewModel>();

        // For Bulk Actions
        public List<Guid> SelectedEmployeeIds { get; set; } = new List<Guid>();
        public Guid? SelectedDepartmentId { get; set; } // For department change
    }
}
