using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseSoftwareVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseSoftwareVersion",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareVersionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSoftwareVersion", x => new { x.CourseId, x.SoftwareVersionId });
                    table.ForeignKey(
                        name: "FK_CourseSoftwareVersion_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSoftwareVersion_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSoftwareVersion_SoftwareVersionId",
                table: "CourseSoftwareVersion",
                column: "SoftwareVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseSoftwareVersion");
        }
    }
}
