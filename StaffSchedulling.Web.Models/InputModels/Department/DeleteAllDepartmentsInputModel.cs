using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Department
{
    public class DeleteAllDepartmentsInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }
    }
}
