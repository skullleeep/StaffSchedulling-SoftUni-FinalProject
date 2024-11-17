using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Models.Dtos
{
    public class CompanyDashboardDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public Guid Invite { get; set; }

        [Required]
        public EmployeeRole Role { get; set; }
    }
}
