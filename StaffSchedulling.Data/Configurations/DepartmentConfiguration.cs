using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            //Make NormalizedName value auto-generate
            builder
                .Property(u => u.NormalizedName)
                .HasComputedColumnSql("UPPER(Name)", stored: true);
        }
    }
}
