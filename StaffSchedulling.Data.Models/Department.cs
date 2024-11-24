using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.Constants.DataConstants.Department;

namespace StaffScheduling.Data.Models
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public Guid CompanyId { get; set; }

        //Navigation

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; }

        public virtual ICollection<EmployeeInfo> DepartmentEmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
