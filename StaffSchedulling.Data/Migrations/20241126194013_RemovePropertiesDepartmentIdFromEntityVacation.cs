using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePropertiesDepartmentIdFromEntityVacation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacations_Departments_DepartmentId",
                table: "Vacations");

            migrationBuilder.DropIndex(
                name: "IX_Vacations_DepartmentId",
                table: "Vacations");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Vacations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Vacations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vacations_DepartmentId",
                table: "Vacations",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacations_Departments_DepartmentId",
                table: "Vacations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }
    }
}
