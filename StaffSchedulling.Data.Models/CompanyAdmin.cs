using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffScheduling.Data.Models
{
    [PrimaryKey(nameof(CompanyId), nameof(AdminId))]
    public class CompanyAdmin
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        public string AdminId { get; set; } = null!;

        //Navigation
        [ForeignKey(nameof(CompanyId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey(nameof(AdminId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser Admin { get; set; } = null!;
    }
}
