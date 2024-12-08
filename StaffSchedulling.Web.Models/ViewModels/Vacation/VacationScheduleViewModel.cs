using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class VacationScheduleViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int Days { get; set; }

        [Required]
        public VacationStatus Status { get; set; } = 0;
    }
}
