using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data.Configurations
{
    internal class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            //Optional one-to-many relationship
            builder
                .HasOne(b => b.Supervisor)
                .WithMany(a => a.DepartmentsWhereSupervisor)
                .HasForeignKey(b => b.SupervisorId)
                .IsRequired(false);
        }
    }
}
