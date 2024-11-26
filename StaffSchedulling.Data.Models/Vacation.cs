using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffScheduling.Data.Models
{
    public class Vacation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

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
    }
}
