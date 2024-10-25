﻿using System.ComponentModel.DataAnnotations;
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

        //Navigation
        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser Owner { get; set; }

        public virtual ICollection<CompanyAdmin> CompaniesAdmins { get; set; } = new HashSet<CompanyAdmin>();
        public virtual ICollection<EmployeeInfo> CompanyEmployees { get; set; } = new HashSet<EmployeeInfo>();
        public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    }
}
