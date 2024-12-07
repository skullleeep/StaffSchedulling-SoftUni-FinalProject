using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common.Enums;
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
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Comment("Used for ordering the vacations")]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int Days { get; set; }

        [Required]
        [Comment("Used for checking if vacation request is still pending or has been approved or denied by a higher-up")]
        public VacationStatus Status { get; set; } = 0;

        //Navigation
        [ForeignKey(nameof(EmployeeId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual EmployeeInfo Employee { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Company Company { get; set; } = null!;
    }
}
