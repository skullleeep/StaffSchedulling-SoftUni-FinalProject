using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class ChangeRoleInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }


        [Required]
        public PermissionRole CurrentEmployeePermission { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public EmployeeRole Role { get; set; }
    }
}
