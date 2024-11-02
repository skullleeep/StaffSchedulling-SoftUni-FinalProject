using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.DataConstants.EmployeeInfo;

namespace StaffScheduling.Data.Models
{
    public class EmployeeInfo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; } = null!;

        [Required]
        public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

        [Required]
        public bool HasJoined { get; set; } = false;

        [Required]
        public Guid CompanyId { get; set; }

        //Foreign Key in EmployeeInfoConfiguration because it needs to be optional
        public Guid? DepartmentId { get; set; } = null;

        //Foreign Key in EmployeeInfoConfiguration because it needs to be optional
        public string? UserId { get; set; } = null;

        //Navigation

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public virtual Department? Department { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
