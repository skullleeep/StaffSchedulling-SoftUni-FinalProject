using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.DataConstants.Company;

namespace StaffScheduling.Data.Models
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

        [Required]
        public Guid Invite { get; set; } = Guid.NewGuid();

        [Required]
        [Comment("Maximum vacation days per year that an employee working at the company is given")]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;

        //Navigation
        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser Owner { get; set; }

        public virtual ICollection<EmployeeInfo> CompanyEmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();
        public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
