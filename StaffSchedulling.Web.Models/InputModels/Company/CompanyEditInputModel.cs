using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.Company;
using static StaffScheduling.Common.ErrorMessages.DataErrorMessages.Company;

namespace StaffScheduling.Web.Models.InputModels.Company
{
    public class CompanyEditInputModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        [Display(Name = "Company Name")]
        [RegularExpression(NameRegexPattern, ErrorMessage = NameError)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(MaxVacationDaysPerYearMinValue, MaxVacationDaysPerYearMaxValue, ErrorMessage = MaxVacationDaysPerYearError)]
        [Display(Name = "Maximum Vacation Days Per Year (For each employee)")]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;
    }
}
