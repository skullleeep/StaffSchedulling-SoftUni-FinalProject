using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffSchedulling.Common.DataConstants.ApplicationUser;

namespace StaffSchedulling.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(FullNameMaxLength)]
        [PersonalData]
        public string? FullName { get; set; }

        [ForeignKey(nameof(Department))]
        public int? DepartmentId { get; set; }

        //Navigation
        public virtual Department Department { get; set; }

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
