using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data.Configurations
{
    public class EmployeeInfoConfiguration : IEntityTypeConfiguration<EmployeeInfo>
    {
        public void Configure(EntityTypeBuilder<EmployeeInfo> builder)
        {
            //Optional one-to-many relationship
            builder
                .HasOne(b => b.User)
                .WithMany(a => a.EmployeeInfoInCompanies)
                .HasForeignKey(b => b.UserId)
                .IsRequired(false);

            //Optional one-to-many relationship
            builder
                .HasOne(b => b.Department)
                .WithMany(d => d.DepartmentEmployeesInfo)
                .HasForeignKey(b => b.DepartmentId)
                .IsRequired(false);

            //Make NormalizedEmail value auto-generate
            builder
                .Property(u => u.NormalizedEmail)
                .HasComputedColumnSql("UPPER(Email)", stored: true);
        }
    }
}
