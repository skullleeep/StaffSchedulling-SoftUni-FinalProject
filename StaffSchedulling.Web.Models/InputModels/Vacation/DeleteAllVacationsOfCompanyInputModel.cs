using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class DeleteAllVacationsOfCompanyInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public VacationStatus VacationStatusToDelete { get; set; }
    }
}
