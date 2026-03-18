using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyFieldsToNeedAndItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalComments",
                table: "TeachingNeeds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsUpdates",
                table: "TeachingNeeds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesiredModifications",
                table: "TeachingNeeds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedStudents",
                table: "TeachingNeeds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoundAllCourses",
                table: "TeachingNeeds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasTechNeeds",
                table: "TeachingNeeds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TeachingNeedItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "TeachingNeedItems",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalComments",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "AllowsUpdates",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "DesiredModifications",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "ExpectedStudents",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "FoundAllCourses",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "HasTechNeeds",
                table: "TeachingNeeds");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TeachingNeedItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "TeachingNeedItems");
        }
    }
}
