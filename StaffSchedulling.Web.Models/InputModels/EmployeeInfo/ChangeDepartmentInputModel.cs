using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class ChangeDepartmentInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        public string? DepartmentName { get; set; }
    }
}
