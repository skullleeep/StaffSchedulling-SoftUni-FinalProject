using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class DeleteVacationOfCompanyInputModel : ManageVacationsFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid VacationId { get; set; }
    }
}
