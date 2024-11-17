using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;
using static StaffScheduling.Common.DataErrorMessages.Company;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyManageViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        [RegularExpression(NameRegexPattern, ErrorMessage = NameError)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(MaxVacationDaysPerYearMinValue, MaxVacationDaysPerYearMaxValue, ErrorMessage = MaxVacationDaysPerYearError)]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;

        [Required]
        public bool UserCanEdit { get; set; } = false;

        [Required]
        public bool UserCanDelete { get; set; } = false;
    }
}
