using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLaboratoryAndOSAndSoftwareVersionOs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OsId",
                table: "SoftwareVersions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Laboratories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatingSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingSystems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersions_OsId",
                table: "SoftwareVersions",
                column: "OsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersions_OperatingSystems_OsId",
                table: "SoftwareVersions",
                column: "OsId",
                principalTable: "OperatingSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersions_OperatingSystems_OsId",
                table: "SoftwareVersions");

            migrationBuilder.DropTable(
                name: "Laboratories");

            migrationBuilder.DropTable(
                name: "OperatingSystems");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersions_OsId",
                table: "SoftwareVersions");

            migrationBuilder.DropColumn(
                name: "OsId",
                table: "SoftwareVersions");
        }
    }
}
