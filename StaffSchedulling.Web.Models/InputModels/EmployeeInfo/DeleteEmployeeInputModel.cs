using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class DeleteEmployeeInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }
    }
}
