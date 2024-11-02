using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;
using static StaffScheduling.Common.DataErrorMessages.Company;

namespace StaffScheduling.Web.Areas.Identity.InputModels
{
    public class CompanyRegisterInputModel : RegisterInputModel
    {

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        [Display(Name = "Company Name")]
        [RegularExpression(NameRegexPattern, ErrorMessage = NameError)]
        public string CompanyName { get; set; } = null!;

        [Required]
        [Range(MaxVacationDaysPerYearMinValue, MaxVacationDaysPerYearMaxValue, ErrorMessage = MaxVacationDaysPerYearError)]
        [Display(Name = "Maximum Vacation Days Per Year (For each employee)")]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;
    }
}
