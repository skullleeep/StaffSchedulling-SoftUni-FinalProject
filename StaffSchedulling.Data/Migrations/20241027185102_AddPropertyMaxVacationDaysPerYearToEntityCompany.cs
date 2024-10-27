using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyMaxVacationDaysPerYearToEntityCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxVacationDaysPerYear",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: Common.DataConstants.Company.MaxVacationDaysPerYearDefaultValue,
                comment: "Maximum vacation days per year that an employee working at the company is given");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxVacationDaysPerYear",
                table: "Companies");
        }
    }
}
