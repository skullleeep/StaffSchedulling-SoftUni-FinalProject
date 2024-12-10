using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class DeleteVacationOfEmployeeInputModel : ManageScheduleFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid VacationId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }
    }
}
