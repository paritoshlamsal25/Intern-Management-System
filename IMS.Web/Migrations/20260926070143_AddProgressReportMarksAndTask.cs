using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressReportMarksAndTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoursWorked",
                table: "ProgressReports");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionFileName",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionFilePath",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionNote",
                table: "TaskItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ProgressReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentReference",
                table: "ProgressReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Marks",
                table: "ProgressReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxMarks",
                table: "ProgressReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "ProgressReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "ProgressReports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_TaskId",
                table: "ProgressReports",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressReports_TaskItems_TaskId",
                table: "ProgressReports",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressReports_TaskItems_TaskId",
                table: "ProgressReports");

            migrationBuilder.DropIndex(
                name: "IX_ProgressReports_TaskId",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "SubmissionFileName",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "SubmissionFilePath",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "SubmissionNote",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "AssignmentReference",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "Marks",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "MaxMarks",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "ProgressReports");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ProgressReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HoursWorked",
                table: "ProgressReports",
                type: "float",
                nullable: true);
        }
    }
}
