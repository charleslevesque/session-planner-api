using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhysicalServerConfiguration");

            migrationBuilder.DropTable(
                name: "PhysicalServerSoftware");

            migrationBuilder.DropTable(
                name: "SoftwareOS");

            migrationBuilder.DropTable(
                name: "VirtualMachineConfiguration");

            migrationBuilder.DropTable(
                name: "VirtualMachineSoftware");

            migrationBuilder.DropTable(
                name: "WorkstationSoftware");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhysicalServerConfiguration",
                columns: table => new
                {
                    PhysicalServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalServerConfiguration", x => new { x.PhysicalServerId, x.ConfigurationId });
                    table.ForeignKey(
                        name: "FK_PhysicalServerConfiguration_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhysicalServerConfiguration_PhysicalServers_PhysicalServerId",
                        column: x => x.PhysicalServerId,
                        principalTable: "PhysicalServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalServerSoftware",
                columns: table => new
                {
                    PhysicalServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalServerSoftware", x => new { x.PhysicalServerId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_PhysicalServerSoftware_PhysicalServers_PhysicalServerId",
                        column: x => x.PhysicalServerId,
                        principalTable: "PhysicalServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhysicalServerSoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareOS",
                columns: table => new
                {
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: false),
                    OSId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareOS", x => new { x.SoftwareId, x.OSId });
                    table.ForeignKey(
                        name: "FK_SoftwareOS_OperatingSystems_OSId",
                        column: x => x.OSId,
                        principalTable: "OperatingSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareOS_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualMachineConfiguration",
                columns: table => new
                {
                    VirtualMachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualMachineConfiguration", x => new { x.VirtualMachineId, x.ConfigurationId });
                    table.ForeignKey(
                        name: "FK_VirtualMachineConfiguration_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VirtualMachineConfiguration_VirtualMachines_VirtualMachineId",
                        column: x => x.VirtualMachineId,
                        principalTable: "VirtualMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualMachineSoftware",
                columns: table => new
                {
                    VirtualMachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualMachineSoftware", x => new { x.VirtualMachineId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_VirtualMachineSoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VirtualMachineSoftware_VirtualMachines_VirtualMachineId",
                        column: x => x.VirtualMachineId,
                        principalTable: "VirtualMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkstationSoftware",
                columns: table => new
                {
                    WorkstationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkstationSoftware", x => new { x.WorkstationId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_WorkstationSoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkstationSoftware_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalServerConfiguration_ConfigurationId",
                table: "PhysicalServerConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalServerSoftware_SoftwareId",
                table: "PhysicalServerSoftware",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareOS_OSId",
                table: "SoftwareOS",
                column: "OSId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualMachineConfiguration_ConfigurationId",
                table: "VirtualMachineConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualMachineSoftware_SoftwareId",
                table: "VirtualMachineSoftware",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSoftware_SoftwareId",
                table: "WorkstationSoftware",
                column: "SoftwareId");
        }
    }
}
