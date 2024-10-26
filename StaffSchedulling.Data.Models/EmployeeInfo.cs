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
        public bool IsAdmin { get; set; } = false;

        [Required]
        public bool IsSuperior { get; set; } = false;

        [Required]
        public bool HasJoined { get; set; } = false;

        [Required]
        public int CompanyId { get; set; }

        //Foreign Key in EmployeeInfoConfiguration because it needs to be optional
        public int? DepartmentId { get; set; } = null;

        //Foreign Key in EmployeeInfoConfiguration because it needs to be optional
        public string? UserId { get; set; }

        //Navigation

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public virtual Department Department { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
