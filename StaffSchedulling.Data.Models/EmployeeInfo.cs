using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffSchedulling.Common.DataConstants.EmployeeInfo;

namespace StaffSchedulling.Data.Models
{
    public class EmployeeInfo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(FullNameMaxLength)]
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; } = null!;

        [Required]
        public int DepartmentId { get; set; }

        //Navigation
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;
    }
}
