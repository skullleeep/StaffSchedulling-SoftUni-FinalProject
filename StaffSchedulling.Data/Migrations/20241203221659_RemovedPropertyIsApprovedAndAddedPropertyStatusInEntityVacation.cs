using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedPropertyIsApprovedAndAddedPropertyStatusInEntityVacation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Vacations");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vacations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Used for checking if vacation request is still pending or has been approved or denied by a higher-up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vacations");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Vacations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Used for checking if vacation request is approved by higher-up");
        }
    }
}
