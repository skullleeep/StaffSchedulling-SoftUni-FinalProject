using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedPropertiesToCompanyEmployeeInfoAndDepartmentEntitiesMakeTheAutoGenerateAndAddUniqueIndexes : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies");

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

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Companies",
                type: "nvarchar(160)",
                maxLength: 160,
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

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId_NormalizedName",
                table: "Companies",
                columns: new[] { "OwnerId", "NormalizedName" },
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

            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId_NormalizedName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "EmployeesInfo");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_CompanyId_Email",
                table: "EmployeesInfo",
                columns: new[] { "CompanyId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies",
                column: "OwnerId");
        }
    }
}
