using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNormalizedNamePropToEntityDepartmentAndAddSameButEmailPropToEntityEmployeeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeesInfo_CompanyId_Email",
                table: "EmployeesInfo");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "EmployeesInfo",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                computedColumnSql: "UPPER(Email)",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Departments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                computedColumnSql: "UPPER(Name)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_CompanyId_NormalizedEmail",
                table: "EmployeesInfo",
                columns: new[] { "CompanyId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId_NormalizedName",
                table: "Departments",
                columns: new[] { "CompanyId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeesInfo_CompanyId_NormalizedEmail",
                table: "EmployeesInfo");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CompanyId_NormalizedName",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "EmployeesInfo");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Departments");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_CompanyId_Email",
                table: "EmployeesInfo",
                columns: new[] { "CompanyId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");
        }
    }
}
