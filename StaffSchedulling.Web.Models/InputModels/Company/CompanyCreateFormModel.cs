using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;
using static StaffScheduling.Common.DataErrorMessages.Company;

namespace StaffScheduling.Web.Models.InputModels.Company
{
    public class CompanyCreateFormModel
    {
        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        [Display(Name = "Company's Name")]
        [RegularExpression(NameRegexPattern, ErrorMessage = NameError)]
        public string Name { get; set; } = null!;

        [EmailAddress]
        [Display(Name = "Admin's Email (Not Required)")]
        public string? AdminEmail { get; set; } = null!;
    }
}
