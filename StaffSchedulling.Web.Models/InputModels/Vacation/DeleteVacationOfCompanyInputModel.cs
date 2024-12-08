using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class DeleteVacationOfCompanyInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid VacationId { get; set; }
    }
}
