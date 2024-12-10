using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class ManageScheduleViewModel : ManageScheduleFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public List<VacationScheduleViewModel> Vacations { get; set; } = new List<VacationScheduleViewModel>();

        [Required]
        public int VacationDaysLeftCurrentYear { get; set; } = 0;

        [Required]
        public int VacationDaysLeftNextYear { get; set; } = 0;

        [Required]
        public int TotalPages { get; set; }
    }
}
