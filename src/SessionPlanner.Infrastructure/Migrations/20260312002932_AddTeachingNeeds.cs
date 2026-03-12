using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeachingNeeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingNeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingNeeds_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingNeeds_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingNeeds_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingNeeds_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeachingNeedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeachingNeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftwareVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    OSId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingNeedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingNeedItems_OperatingSystems_OSId",
                        column: x => x.OSId,
                        principalTable: "OperatingSystems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingNeedItems_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingNeedItems_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingNeedItems_TeachingNeeds_TeachingNeedId",
                        column: x => x.TeachingNeedId,
                        principalTable: "TeachingNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeedItems_OSId",
                table: "TeachingNeedItems",
                column: "OSId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeedItems_SoftwareId",
                table: "TeachingNeedItems",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeedItems_SoftwareVersionId",
                table: "TeachingNeedItems",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeedItems_TeachingNeedId",
                table: "TeachingNeedItems",
                column: "TeachingNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeeds_CourseId",
                table: "TeachingNeeds",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeeds_PersonnelId",
                table: "TeachingNeeds",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeeds_ReviewedByUserId",
                table: "TeachingNeeds",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNeeds_SessionId",
                table: "TeachingNeeds",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingNeedItems");

            migrationBuilder.DropTable(
                name: "TeachingNeeds");
        }
    }
}
