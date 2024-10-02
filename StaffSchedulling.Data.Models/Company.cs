using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffSchedulling.Common.DataConstants.Company;

namespace StaffSchedulling.Data.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(NameMaxLength)]
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string OwnerId { get; set; } = null!;

        public string? AdminId { get; set; }

        //Navigation
        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser Owner { get; set; }

        [ForeignKey(nameof(AdminId))]
        public ApplicationUser Admin { get; set; }

        public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    }
}
