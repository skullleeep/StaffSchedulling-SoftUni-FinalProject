using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyDashboardViewModel
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
        public bool UserCanManage { get; set; } = false;
    }
}
