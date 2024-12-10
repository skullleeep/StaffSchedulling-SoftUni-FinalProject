using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class ChangeRoleInputModel : ManageEmployeesInfoFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public PermissionRole CurrentUserPermission { get; set; } = PermissionRole.None;

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public EmployeeRole Role { get; set; }
    }
}
