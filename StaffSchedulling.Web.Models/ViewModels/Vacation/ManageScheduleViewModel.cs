using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class ManageScheduleViewModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public List<VacationViewModel> Vacations { get; set; } = new List<VacationViewModel>();

        public VacationSortFilter? SortFilter { get; set; }

        [Required]
        public int VacationDaysLeftCurrentYear { get; set; } = 0;

        [Required]
        public int VacationDaysLeftNextYear { get; set; } = 0;

        [Required]
        public int CurrentPage { get; set; } = 1;

        [Required]
        public int TotalPages { get; set; }
    }
}
