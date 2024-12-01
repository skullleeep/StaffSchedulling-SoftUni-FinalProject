using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            //Make NormalizedName value auto-generate
            builder
                .Property(u => u.NormalizedName)
                .HasComputedColumnSql("UPPER(Name)", stored: true);
        }
    }
}
