using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.FilterModels
{
    public class ManageDepartmentsFilters
    {
        //Purely for returning the page back to original after redirect

        [Required]
        public int CurrentPage { get; set; } = 1;
    }
}
