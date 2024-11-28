using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class DeleteAllEmployeesInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }
    }
}
