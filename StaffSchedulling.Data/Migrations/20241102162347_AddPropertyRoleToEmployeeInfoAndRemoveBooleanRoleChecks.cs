using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyRoleToEmployeeInfoAndRemoveBooleanRoleChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "EmployeesInfo");

            migrationBuilder.DropColumn(
                name: "IsSuperior",
                table: "EmployeesInfo");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "EmployeesInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "EmployeesInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "EmployeesInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperior",
                table: "EmployeesInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
