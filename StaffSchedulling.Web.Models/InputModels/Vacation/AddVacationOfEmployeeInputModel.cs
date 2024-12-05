using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class AddVacationOfEmployeeInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }
}
