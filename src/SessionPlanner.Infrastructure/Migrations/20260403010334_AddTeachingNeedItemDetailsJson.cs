using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingNeedItemDetailsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailsJson",
                table: "TeachingNeedItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailsJson",
                table: "TeachingNeedItems");
        }
    }
}
