using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.Department;

namespace StaffScheduling.Web.Models.ViewModels.Department
{
    public class DepartmentManageViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public int EmployeeCount { get; set; }
    }
}
