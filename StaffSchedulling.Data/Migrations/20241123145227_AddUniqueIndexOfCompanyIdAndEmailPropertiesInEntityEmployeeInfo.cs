using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOfCompanyIdAndEmailPropertiesInEntityEmployeeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeesInfo_CompanyId",
                table: "EmployeesInfo");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_CompanyId_Email",
                table: "EmployeesInfo",
                columns: new[] { "CompanyId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeesInfo_CompanyId_Email",
                table: "EmployeesInfo");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_CompanyId",
                table: "EmployeesInfo",
                column: "CompanyId");
        }
    }
}
