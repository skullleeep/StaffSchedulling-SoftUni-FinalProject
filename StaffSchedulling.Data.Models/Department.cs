using System.ComponentModel.DataAnnotations;
using static StaffSchedulling.Common.DataConstants.Department;

namespace StaffSchedulling.Data.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        //Navigation

        public virtual ICollection<EmployeeInfo> EmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();

        public virtual ICollection<ApplicationUser> Employees { get; set; } = new HashSet<ApplicationUser>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
