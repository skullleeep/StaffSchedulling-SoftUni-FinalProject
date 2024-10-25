using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.ApplicationUser;

namespace StaffScheduling.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(FullNameMaxLength)]
        [PersonalData]
        public string? FullName { get; set; }

        //Navigation
        public virtual ICollection<Company> CompaniesOwned { get; set; } = new HashSet<Company>();

        public virtual ICollection<CompanyAdmin> CompaniesAdmins { get; set; } = new HashSet<CompanyAdmin>();

        public virtual ICollection<Department> DepartmentsWhereSupervisor { get; set; } = new HashSet<Department>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
