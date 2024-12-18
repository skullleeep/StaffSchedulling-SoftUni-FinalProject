﻿using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.Company;
using static StaffScheduling.Common.ErrorMessages.DataErrorMessages.Company;

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
        public Guid Invite { get; set; }

        [Required]
        [Range(MaxVacationDaysPerYearMinValue, MaxVacationDaysPerYearMaxValue, ErrorMessage = MaxVacationDaysPerYearError)]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;

        [Required]
        public bool UserCanEdit { get; set; } = false;

        [Required]
        public bool UserCanDelete { get; set; } = false;
    }
}
