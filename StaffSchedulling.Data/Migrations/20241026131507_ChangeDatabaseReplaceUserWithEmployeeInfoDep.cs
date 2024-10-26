using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDatabaseReplaceUserWithEmployeeInfoDep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_AdminId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_AspNetUsers_SupervisorId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacations_AspNetUsers_EmployeeId",
                table: "Vacations");

            migrationBuilder.DropIndex(
                name: "IX_Departments_SupervisorId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Companies_AdminId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "VacationId",
                table: "Vacations",
                newName: "CompanyId");

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "Vacations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "EmployeesInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "EmployeesInfo",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vacations_CompanyId",
                table: "Vacations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesInfo_UserId",
                table: "EmployeesInfo",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeesInfo_AspNetUsers_UserId",
                table: "EmployeesInfo",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacations_Companies_CompanyId",
                table: "Vacations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacations_EmployeesInfo_EmployeeId",
                table: "Vacations",
                column: "EmployeeId",
                principalTable: "EmployeesInfo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeesInfo_AspNetUsers_UserId",
                table: "EmployeesInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacations_Companies_CompanyId",
                table: "Vacations");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacations_EmployeesInfo_EmployeeId",
                table: "Vacations");

            migrationBuilder.DropIndex(
                name: "IX_Vacations_CompanyId",
                table: "Vacations");

            migrationBuilder.DropIndex(
                name: "IX_EmployeesInfo_UserId",
                table: "EmployeesInfo");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "EmployeesInfo");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmployeesInfo");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Vacations",
                newName: "VacationId");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Vacations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "SupervisorId",
                table: "Departments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                table: "Companies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SupervisorId",
                table: "Departments",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_AdminId",
                table: "Companies",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_AdminId",
                table: "Companies",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_AspNetUsers_SupervisorId",
                table: "Departments",
                column: "SupervisorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacations_AspNetUsers_EmployeeId",
                table: "Vacations",
                column: "EmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
