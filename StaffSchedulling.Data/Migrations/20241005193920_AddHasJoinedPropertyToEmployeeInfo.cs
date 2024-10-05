using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHasJoinedPropertyToEmployeeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasJoined",
                table: "EmployeesInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasJoined",
                table: "EmployeesInfo");
        }
    }
}
