using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.DataConstants.ApplicationUser;

namespace StaffScheduling.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(FullNameMaxLength)]
        [PersonalData]
        public string? FullName { get; set; }

        //Navigation
        [InverseProperty("Owner")]
        public virtual ICollection<Company> CompaniesOwned { get; set; } = new HashSet<Company>();

        [InverseProperty("Admin")]
        public virtual ICollection<Company> CompaniesWhereAdmin { get; set; } = new HashSet<Company>();

        public virtual ICollection<Department> DepartmentsWhereSupervisor { get; set; } = new HashSet<Department>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
