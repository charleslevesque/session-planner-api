using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workstations_OperatingSystems_OperatingSystemId",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "Workstations");

            migrationBuilder.RenameColumn(
                name: "OperatingSystemId",
                table: "Workstations",
                newName: "OSId");

            migrationBuilder.RenameIndex(
                name: "IX_Workstations_OperatingSystemId",
                table: "Workstations",
                newName: "IX_Workstations_OSId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Workstations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstallCommand",
                table: "Softwares",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultAccessories = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personnel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Function = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hostname = table.Column<string>(type: "TEXT", nullable: false),
                    CpuCores = table.Column<int>(type: "INTEGER", nullable: false),
                    RamGb = table.Column<int>(type: "INTEGER", nullable: false),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessType = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    OSId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalServers_OperatingSystems_OSId",
                        column: x => x.OSId,
                        principalTable: "OperatingSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaaSProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaaSProducts", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "ConfigurationOS",
                columns: table => new
                {
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    OSId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationOS", x => new { x.ConfigurationId, x.OSId });
                    table.ForeignKey(
                        name: "FK_ConfigurationOS_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationOS_OperatingSystems_OSId",
                        column: x => x.OSId,
                        principalTable: "OperatingSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaboratoryConfiguration",
                columns: table => new
                {
                    LaboratoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratoryConfiguration", x => new { x.LaboratoryId, x.ConfigurationId });
                    table.ForeignKey(
                        name: "FK_LaboratoryConfiguration_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LaboratoryConfiguration_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseConfiguration",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseConfiguration", x => new { x.CourseId, x.ConfigurationId });
                    table.ForeignKey(
                        name: "FK_CourseConfiguration_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseConfiguration_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseLaboratory",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    LaboratoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLaboratory", x => new { x.CourseId, x.LaboratoryId });
                    table.ForeignKey(
                        name: "FK_CourseLaboratory_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseLaboratory_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseSoftware",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSoftware", x => new { x.CourseId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_CourseSoftware_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSoftware_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseEquipmentModel",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentModelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEquipmentModel", x => new { x.CourseId, x.EquipmentModelId });
                    table.ForeignKey(
                        name: "FK_CourseEquipmentModel_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEquipmentModel_EquipmentModels_EquipmentModelId",
                        column: x => x.EquipmentModelId,
                        principalTable: "EquipmentModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoursePersonnel",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePersonnel", x => new { x.CourseId, x.PersonnelId });
                    table.ForeignKey(
                        name: "FK_CoursePersonnel_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursePersonnel_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoursePhysicalServer",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    PhysicalServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePhysicalServer", x => new { x.CourseId, x.PhysicalServerId });
                    table.ForeignKey(
                        name: "FK_CoursePhysicalServer_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursePhysicalServer_PhysicalServers_PhysicalServerId",
                        column: x => x.PhysicalServerId,
                        principalTable: "PhysicalServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "VirtualMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CpuCores = table.Column<int>(type: "INTEGER", nullable: false),
                    RamGb = table.Column<int>(type: "INTEGER", nullable: false),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessType = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    OSId = table.Column<int>(type: "INTEGER", nullable: false),
                    HostServerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualMachines_OperatingSystems_OSId",
                        column: x => x.OSId,
                        principalTable: "OperatingSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VirtualMachines_PhysicalServers_HostServerId",
                        column: x => x.HostServerId,
                        principalTable: "PhysicalServers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseSaaSProduct",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaaSProductId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSaaSProduct", x => new { x.CourseId, x.SaaSProductId });
                    table.ForeignKey(
                        name: "FK_CourseSaaSProduct_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSaaSProduct_SaaSProducts_SaaSProductId",
                        column: x => x.SaaSProductId,
                        principalTable: "SaaSProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseVirtualMachine",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    VirtualMachineId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVirtualMachine", x => new { x.CourseId, x.VirtualMachineId });
                    table.ForeignKey(
                        name: "FK_CourseVirtualMachine_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseVirtualMachine_VirtualMachines_VirtualMachineId",
                        column: x => x.VirtualMachineId,
                        principalTable: "VirtualMachines",
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

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationOS_OSId",
                table: "ConfigurationOS",
                column: "OSId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseConfiguration_ConfigurationId",
                table: "CourseConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEquipmentModel_EquipmentModelId",
                table: "CourseEquipmentModel",
                column: "EquipmentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLaboratory_LaboratoryId",
                table: "CourseLaboratory",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePersonnel_PersonnelId",
                table: "CoursePersonnel",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePhysicalServer_PhysicalServerId",
                table: "CoursePhysicalServer",
                column: "PhysicalServerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSaaSProduct_SaaSProductId",
                table: "CourseSaaSProduct",
                column: "SaaSProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSoftware_SoftwareId",
                table: "CourseSoftware",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVirtualMachine_VirtualMachineId",
                table: "CourseVirtualMachine",
                column: "VirtualMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryConfiguration_ConfigurationId",
                table: "LaboratoryConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalServerConfiguration_ConfigurationId",
                table: "PhysicalServerConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalServers_OSId",
                table: "PhysicalServers",
                column: "OSId");

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
                name: "IX_VirtualMachines_HostServerId",
                table: "VirtualMachines",
                column: "HostServerId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualMachines_OSId",
                table: "VirtualMachines",
                column: "OSId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualMachineSoftware_SoftwareId",
                table: "VirtualMachineSoftware",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSoftware_SoftwareId",
                table: "WorkstationSoftware",
                column: "SoftwareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workstations_OperatingSystems_OSId",
                table: "Workstations",
                column: "OSId",
                principalTable: "OperatingSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workstations_OperatingSystems_OSId",
                table: "Workstations");

            migrationBuilder.DropTable(
                name: "ConfigurationOS");

            migrationBuilder.DropTable(
                name: "CourseConfiguration");

            migrationBuilder.DropTable(
                name: "CourseEquipmentModel");

            migrationBuilder.DropTable(
                name: "CourseLaboratory");

            migrationBuilder.DropTable(
                name: "CoursePersonnel");

            migrationBuilder.DropTable(
                name: "CoursePhysicalServer");

            migrationBuilder.DropTable(
                name: "CourseSaaSProduct");

            migrationBuilder.DropTable(
                name: "CourseSoftware");

            migrationBuilder.DropTable(
                name: "CourseVirtualMachine");

            migrationBuilder.DropTable(
                name: "LaboratoryConfiguration");

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

            migrationBuilder.DropTable(
                name: "EquipmentModels");

            migrationBuilder.DropTable(
                name: "Personnel");

            migrationBuilder.DropTable(
                name: "SaaSProducts");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "VirtualMachines");

            migrationBuilder.DropTable(
                name: "PhysicalServers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "InstallCommand",
                table: "Softwares");

            migrationBuilder.RenameColumn(
                name: "OSId",
                table: "Workstations",
                newName: "OperatingSystemId");

            migrationBuilder.RenameIndex(
                name: "IX_Workstations_OSId",
                table: "Workstations",
                newName: "IX_Workstations_OperatingSystemId");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "Workstations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Workstations_OperatingSystems_OperatingSystemId",
                table: "Workstations",
                column: "OperatingSystemId",
                principalTable: "OperatingSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
