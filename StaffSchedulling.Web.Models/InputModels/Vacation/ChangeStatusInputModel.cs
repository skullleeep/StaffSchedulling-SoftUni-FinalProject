using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class ChangeStatusInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid VacationId { get; set; }

        [Required]
        public VacationStatus Status { get; set; }
    }
}
