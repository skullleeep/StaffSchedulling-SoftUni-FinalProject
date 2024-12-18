﻿using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.FilterConstants;

namespace StaffScheduling.Web.Models.FilterModels
{
    public class ManageEmployeesInfoFilters
    {
        //Purely for returning the page back to original after redirect
        [MaxLength(SearchQueryMaxLength)]
        public string? SearchQuery { get; set; }

        public EmployeeSearchFilter? SearchFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;
    }
}
