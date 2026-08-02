using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MentorId",
                table: "Interns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Mentors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interns_MentorId",
                table: "Interns",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interns_Mentors_MentorId",
                table: "Interns",
                column: "MentorId",
                principalTable: "Mentors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interns_Mentors_MentorId",
                table: "Interns");

            migrationBuilder.DropTable(
                name: "Mentors");

            migrationBuilder.DropIndex(
                name: "IX_Interns_MentorId",
                table: "Interns");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Interns");
        }
    }
}
