using StaffScheduling.Web.Models.FilterModels;
using StaffScheduling.Web.Models.ViewModels.Department;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class ChangeDepartmentInputModel : ManageEmployeesInfoFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        [Required]
        public Guid EmployeeId { get; set; }

        public string CurrentDepartmentName { get; set; } = String.Empty;

        public Guid SelectedDepartmentId { get; set; } = Guid.Empty;
    }
}
