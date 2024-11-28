using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.Constants.DataConstants.Department;

namespace StaffScheduling.Data.Models
{
    [Index(nameof(CompanyId), nameof(NormalizedName), IsUnique = true)]
    public class Department
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        //Auto-generation in DepartmentConfiguration
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(NameMaxLength)]
        public string NormalizedName { get; set; } = null!;

        [Required]
        public Guid CompanyId { get; set; }

        //Navigation

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public virtual ICollection<EmployeeInfo> DepartmentEmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();
    }
}
