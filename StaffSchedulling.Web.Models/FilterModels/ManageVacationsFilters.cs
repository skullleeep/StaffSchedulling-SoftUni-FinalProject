using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.FilterModels
{
    public class ManageVacationsFilters
    {
        //Purely for returning the page back to original after redirect
        public string? SearchQuery { get; set; }

        public VacationSortFilter? SortFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;
    }
}
