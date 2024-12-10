using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Department
{
    public class DeleteDepartmentInputModel : ManageDepartmentsFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }
    }
}
