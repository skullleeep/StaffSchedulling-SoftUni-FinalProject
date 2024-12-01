using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.Constants.DataConstants.Company;

namespace StaffScheduling.Data.Models
{
    [Index(nameof(OwnerId), nameof(NormalizedName), IsUnique = true)]
    public class Company
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        //Auto-generation in CompanyConfiguration
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(NameMaxLength)]
        public string NormalizedName { get; set; } = null!;

        [Required]
        public string OwnerId { get; set; } = null!;

        [Required]
        public Guid Invite { get; set; } = Guid.NewGuid();

        [Required]
        [Comment("Maximum vacation days per year that an employee working at the company is given")]
        public int MaxVacationDaysPerYear { get; set; } = MaxVacationDaysPerYearDefaultValue;

        //Navigation
        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser Owner { get; set; } = null!;

        public virtual ICollection<EmployeeInfo> CompanyEmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();
        public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
