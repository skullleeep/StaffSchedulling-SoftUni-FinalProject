using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.FilterModels
{
    public class ManageEmployeesInfoFilters
    {
        //Purely for returning the page back to original after redirect
        public string? SearchQuery { get; set; }

        public EmployeeSearchFilter? SearchFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;
    }
}
