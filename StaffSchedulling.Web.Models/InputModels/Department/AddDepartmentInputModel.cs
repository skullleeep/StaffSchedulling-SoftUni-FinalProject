using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.Department;

namespace StaffScheduling.Web.Models.InputModels.Department
{
    public class AddDepartmentInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;
    }
}
