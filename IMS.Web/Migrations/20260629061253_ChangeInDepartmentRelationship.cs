using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInDepartmentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interns_Departments_DepartmentId",
                table: "Interns");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Interns",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Interns_Departments_DepartmentId",
                table: "Interns",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interns_Departments_DepartmentId",
                table: "Interns");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Interns",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Interns_Departments_DepartmentId",
                table: "Interns",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
