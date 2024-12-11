using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.FilterConstants;

namespace StaffScheduling.Web.Models.FilterModels
{
    public class ManageVacationsFilters
    {
        //Purely for returning the page back to original after redirect
        [MaxLength(SearchQueryMaxLength)]
        public string? SearchQuery { get; set; }

        public VacationSortFilter? SortFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;
    }
}
