﻿using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(MaxVacationDaysPerYearMinValue, MaxVacationDaysPerYearMaxValue)]
        public int MaxVacationDaysPerYear { get; set; }

        [Required]
        public Guid Invite { get; set; }
    }
}
