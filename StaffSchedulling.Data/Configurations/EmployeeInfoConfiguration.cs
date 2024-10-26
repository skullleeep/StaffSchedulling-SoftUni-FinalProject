﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Data.Configurations
{
    internal class EmployeeInfoConfiguration : IEntityTypeConfiguration<EmployeeInfo>
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
        }
    }
}