﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static StaffScheduling.Common.DataConstants.Department;

namespace StaffScheduling.Data.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public int CompanyId { get; set; }

        public string? SupervisorId { get; set; }

        //Navigation
        public virtual ApplicationUser Supervisor { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; }

        public virtual ICollection<EmployeeInfo> DepartmentEmployeesInfo { get; set; } = new HashSet<EmployeeInfo>();

        public virtual ICollection<Vacation> Vacations { get; set; } = new HashSet<Vacation>();
    }
}
