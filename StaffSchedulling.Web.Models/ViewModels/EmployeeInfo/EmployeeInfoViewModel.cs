using StaffScheduling.Common.Constants;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.EmployeeInfo;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Models.ViewModels.EmployeeInfo
{
    public class EmployeeInfoViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [MinLength(DataConstants.ApplicationUser.FullNameMinLength)]
        [MaxLength(DataConstants.ApplicationUser.FullNameMaxLength)]
        public string? Name { get; set; }

        [Required]
        [MinLength(EmailMinLength)]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; } = null!;

        [Required]
        public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

        [MinLength(DataConstants.Department.NameMinLength)]
        [MaxLength(DataConstants.Department.NameMaxLength)]
        public string? Department { get; set; }

        public bool HasJoined { get; set; } = false;
    }
}
