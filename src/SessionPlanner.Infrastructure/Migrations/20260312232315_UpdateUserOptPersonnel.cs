using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserOptPersonnel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonnelId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonnelId",
                table: "Users",
                column: "PersonnelId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Personnel_PersonnelId",
                table: "Users",
                column: "PersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Personnel_PersonnelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PersonnelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonnelId",
                table: "Users");
        }
    }
}
