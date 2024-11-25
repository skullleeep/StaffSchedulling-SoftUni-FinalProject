using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.EmployeeInfo;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class AddEmployeeInfoManuallyInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [EmailAddress]
        [MinLength(EmailMinLength)]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; } = null!;
    }
}
