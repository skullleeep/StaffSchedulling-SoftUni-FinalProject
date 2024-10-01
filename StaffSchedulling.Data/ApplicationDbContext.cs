using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffSchedulling.Data.Models;

namespace StaffSchedulling.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<Department> Departments { get; set; }

        public virtual DbSet<EmployeeInfo> EmployeesInfo { get; set; }

        public virtual DbSet<Vacation> Vacations { get; set; }
    }
}
