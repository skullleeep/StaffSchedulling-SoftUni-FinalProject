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
        public bool IsSuperior { get; set; } = false;

        [Required]
        public bool HasJoined { get; set; } = false;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(Department))]
        public int? DepartmentId { get; set; }

        //Navigation

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public virtual Department Department { get; set; }
    }
}
