using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class VacationViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public int Days { get; set; }

        [Required]
        public VacationStatus Status { get; set; } = 0;
    }
}
