using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data.Configurations;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<Company> Companies { get; set; }

        public virtual DbSet<Department> Departments { get; set; }

        public virtual DbSet<EmployeeInfo> EmployeesInfo { get; set; }

        public virtual DbSet<Vacation> Vacations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CompanyConfiguration());
            builder.ApplyConfiguration(new DepartmentConfiguration());
            builder.ApplyConfiguration(new EmployeeInfoConfiguration());

            base.OnModelCreating(builder);
        }
    }
}
