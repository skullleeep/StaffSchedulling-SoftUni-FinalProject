﻿using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.Company;

namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyJoinViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public Guid Invite { get; set; }
    }
}