using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffScheduling.Data.Models
{
    public class Vacation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public int Days { get; set; }

        [Required]
        [Comment("Used for checking if vacation request is approved by higher-up")]
        public bool IsApproved { get; set; } = false;

        //Navigation
        [ForeignKey(nameof(EmployeeId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual EmployeeInfo Employee { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Department Department { get; set; } = null!;
    }
}
