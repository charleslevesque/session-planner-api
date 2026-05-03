using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationOS_Configurations_ConfigurationId",
                table: "ConfigurationOS");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationOS_OperatingSystems_OSId",
                table: "ConfigurationOS");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseConfiguration_Configurations_ConfigurationId",
                table: "CourseConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseConfiguration_Courses_CourseId",
                table: "CourseConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEquipmentModel_Courses_CourseId",
                table: "CourseEquipmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEquipmentModel_EquipmentModels_EquipmentModelId",
                table: "CourseEquipmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseLaboratory_Courses_CourseId",
                table: "CourseLaboratory");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseLaboratory_Laboratories_LaboratoryId",
                table: "CourseLaboratory");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePersonnel_Courses_CourseId",
                table: "CoursePersonnel");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePersonnel_Personnel_PersonnelId",
                table: "CoursePersonnel");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePhysicalServer_Courses_CourseId",
                table: "CoursePhysicalServer");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePhysicalServer_PhysicalServers_PhysicalServerId",
                table: "CoursePhysicalServer");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSaaSProduct_Courses_CourseId",
                table: "CourseSaaSProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSaaSProduct_SaaSProducts_SaaSProductId",
                table: "CourseSaaSProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftware_Courses_CourseId",
                table: "CourseSoftware");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftware_Softwares_SoftwareId",
                table: "CourseSoftware");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwareVersion_Courses_CourseId",
                table: "CourseSoftwareVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwareVersion_SoftwareVersions_SoftwareVersionId",
                table: "CourseSoftwareVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachine_Courses_CourseId",
                table: "CourseVirtualMachine");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachine_VirtualMachines_VirtualMachineId",
                table: "CourseVirtualMachine");

            migrationBuilder.DropForeignKey(
                name: "FK_LaboratoryConfiguration_Configurations_ConfigurationId",
                table: "LaboratoryConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_LaboratoryConfiguration_Laboratories_LaboratoryId",
                table: "LaboratoryConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeachingNeeds_Users_ReviewedByUserId",
                table: "TeachingNeeds");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LaboratoryConfiguration",
                table: "LaboratoryConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseVirtualMachine",
                table: "CourseVirtualMachine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSoftwareVersion",
                table: "CourseSoftwareVersion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSoftware",
                table: "CourseSoftware");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSaaSProduct",
                table: "CourseSaaSProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoursePhysicalServer",
                table: "CoursePhysicalServer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoursePersonnel",
                table: "CoursePersonnel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseLaboratory",
                table: "CourseLaboratory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseEquipmentModel",
                table: "CourseEquipmentModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseConfiguration",
                table: "CourseConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfigurationOS",
                table: "ConfigurationOS");

            migrationBuilder.RenameTable(
                name: "LaboratoryConfiguration",
                newName: "LaboratoryConfigurations");

            migrationBuilder.RenameTable(
                name: "CourseVirtualMachine",
                newName: "CourseVirtualMachines");

            migrationBuilder.RenameTable(
                name: "CourseSoftwareVersion",
                newName: "CourseSoftwareVersions");

            migrationBuilder.RenameTable(
                name: "CourseSoftware",
                newName: "CourseSoftwares");

            migrationBuilder.RenameTable(
                name: "CourseSaaSProduct",
                newName: "CourseSaaSProducts");

            migrationBuilder.RenameTable(
                name: "CoursePhysicalServer",
                newName: "CoursePhysicalServers");

            migrationBuilder.RenameTable(
                name: "CoursePersonnel",
                newName: "CoursePersonnels");

            migrationBuilder.RenameTable(
                name: "CourseLaboratory",
                newName: "CourseLaboratories");

            migrationBuilder.RenameTable(
                name: "CourseEquipmentModel",
                newName: "CourseEquipmentModels");

            migrationBuilder.RenameTable(
                name: "CourseConfiguration",
                newName: "CourseConfigurations");

            migrationBuilder.RenameTable(
                name: "ConfigurationOS",
                newName: "ConfigurationOSes");

            migrationBuilder.RenameIndex(
                name: "IX_LaboratoryConfiguration_ConfigurationId",
                table: "LaboratoryConfigurations",
                newName: "IX_LaboratoryConfigurations_ConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVirtualMachine_VirtualMachineId",
                table: "CourseVirtualMachines",
                newName: "IX_CourseVirtualMachines_VirtualMachineId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSoftwareVersion_SoftwareVersionId",
                table: "CourseSoftwareVersions",
                newName: "IX_CourseSoftwareVersions_SoftwareVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSoftware_SoftwareId",
                table: "CourseSoftwares",
                newName: "IX_CourseSoftwares_SoftwareId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSaaSProduct_SaaSProductId",
                table: "CourseSaaSProducts",
                newName: "IX_CourseSaaSProducts_SaaSProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePhysicalServer_PhysicalServerId",
                table: "CoursePhysicalServers",
                newName: "IX_CoursePhysicalServers_PhysicalServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePersonnel_PersonnelId",
                table: "CoursePersonnels",
                newName: "IX_CoursePersonnels_PersonnelId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseLaboratory_LaboratoryId",
                table: "CourseLaboratories",
                newName: "IX_CourseLaboratories_LaboratoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseEquipmentModel_EquipmentModelId",
                table: "CourseEquipmentModels",
                newName: "IX_CourseEquipmentModels_EquipmentModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseConfiguration_ConfigurationId",
                table: "CourseConfigurations",
                newName: "IX_CourseConfigurations_ConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationOS_OSId",
                table: "ConfigurationOSes",
                newName: "IX_ConfigurationOSes_OSId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LaboratoryConfigurations",
                table: "LaboratoryConfigurations",
                columns: new[] { "LaboratoryId", "ConfigurationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseVirtualMachines",
                table: "CourseVirtualMachines",
                columns: new[] { "CourseId", "VirtualMachineId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSoftwareVersions",
                table: "CourseSoftwareVersions",
                columns: new[] { "CourseId", "SoftwareVersionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSoftwares",
                table: "CourseSoftwares",
                columns: new[] { "CourseId", "SoftwareId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSaaSProducts",
                table: "CourseSaaSProducts",
                columns: new[] { "CourseId", "SaaSProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoursePhysicalServers",
                table: "CoursePhysicalServers",
                columns: new[] { "CourseId", "PhysicalServerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoursePersonnels",
                table: "CoursePersonnels",
                columns: new[] { "CourseId", "PersonnelId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseLaboratories",
                table: "CourseLaboratories",
                columns: new[] { "CourseId", "LaboratoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseEquipmentModels",
                table: "CourseEquipmentModels",
                columns: new[] { "CourseId", "EquipmentModelId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseConfigurations",
                table: "CourseConfigurations",
                columns: new[] { "CourseId", "ConfigurationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfigurationOSes",
                table: "ConfigurationOSes",
                columns: new[] { "ConfigurationId", "OSId" });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PersonnelId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PersonnelId",
                table: "AspNetUsers",
                column: "PersonnelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationOSes_Configurations_ConfigurationId",
                table: "ConfigurationOSes",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationOSes_OperatingSystems_OSId",
                table: "ConfigurationOSes",
                column: "OSId",
                principalTable: "OperatingSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseConfigurations_Configurations_ConfigurationId",
                table: "CourseConfigurations",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseConfigurations_Courses_CourseId",
                table: "CourseConfigurations",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEquipmentModels_Courses_CourseId",
                table: "CourseEquipmentModels",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEquipmentModels_EquipmentModels_EquipmentModelId",
                table: "CourseEquipmentModels",
                column: "EquipmentModelId",
                principalTable: "EquipmentModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLaboratories_Courses_CourseId",
                table: "CourseLaboratories",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLaboratories_Laboratories_LaboratoryId",
                table: "CourseLaboratories",
                column: "LaboratoryId",
                principalTable: "Laboratories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePersonnels_Courses_CourseId",
                table: "CoursePersonnels",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePersonnels_Personnel_PersonnelId",
                table: "CoursePersonnels",
                column: "PersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePhysicalServers_Courses_CourseId",
                table: "CoursePhysicalServers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePhysicalServers_PhysicalServers_PhysicalServerId",
                table: "CoursePhysicalServers",
                column: "PhysicalServerId",
                principalTable: "PhysicalServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSaaSProducts_Courses_CourseId",
                table: "CourseSaaSProducts",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSaaSProducts_SaaSProducts_SaaSProductId",
                table: "CourseSaaSProducts",
                column: "SaaSProductId",
                principalTable: "SaaSProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwares_Courses_CourseId",
                table: "CourseSoftwares",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwares_Softwares_SoftwareId",
                table: "CourseSoftwares",
                column: "SoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwareVersions_Courses_CourseId",
                table: "CourseSoftwareVersions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwareVersions_SoftwareVersions_SoftwareVersionId",
                table: "CourseSoftwareVersions",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachines_Courses_CourseId",
                table: "CourseVirtualMachines",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachines_VirtualMachines_VirtualMachineId",
                table: "CourseVirtualMachines",
                column: "VirtualMachineId",
                principalTable: "VirtualMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LaboratoryConfigurations_Configurations_ConfigurationId",
                table: "LaboratoryConfigurations",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LaboratoryConfigurations_Laboratories_LaboratoryId",
                table: "LaboratoryConfigurations",
                column: "LaboratoryId",
                principalTable: "Laboratories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_AspNetUsers_CreatedByUserId",
                table: "Sessions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingNeeds_AspNetUsers_ReviewedByUserId",
                table: "TeachingNeeds",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationOSes_Configurations_ConfigurationId",
                table: "ConfigurationOSes");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationOSes_OperatingSystems_OSId",
                table: "ConfigurationOSes");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseConfigurations_Configurations_ConfigurationId",
                table: "CourseConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseConfigurations_Courses_CourseId",
                table: "CourseConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEquipmentModels_Courses_CourseId",
                table: "CourseEquipmentModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEquipmentModels_EquipmentModels_EquipmentModelId",
                table: "CourseEquipmentModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseLaboratories_Courses_CourseId",
                table: "CourseLaboratories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseLaboratories_Laboratories_LaboratoryId",
                table: "CourseLaboratories");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePersonnels_Courses_CourseId",
                table: "CoursePersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePersonnels_Personnel_PersonnelId",
                table: "CoursePersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePhysicalServers_Courses_CourseId",
                table: "CoursePhysicalServers");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePhysicalServers_PhysicalServers_PhysicalServerId",
                table: "CoursePhysicalServers");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSaaSProducts_Courses_CourseId",
                table: "CourseSaaSProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSaaSProducts_SaaSProducts_SaaSProductId",
                table: "CourseSaaSProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwares_Courses_CourseId",
                table: "CourseSoftwares");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwares_Softwares_SoftwareId",
                table: "CourseSoftwares");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwareVersions_Courses_CourseId",
                table: "CourseSoftwareVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSoftwareVersions_SoftwareVersions_SoftwareVersionId",
                table: "CourseSoftwareVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachines_Courses_CourseId",
                table: "CourseVirtualMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVirtualMachines_VirtualMachines_VirtualMachineId",
                table: "CourseVirtualMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_LaboratoryConfigurations_Configurations_ConfigurationId",
                table: "LaboratoryConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_LaboratoryConfigurations_Laboratories_LaboratoryId",
                table: "LaboratoryConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_AspNetUsers_CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeachingNeeds_AspNetUsers_ReviewedByUserId",
                table: "TeachingNeeds");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LaboratoryConfigurations",
                table: "LaboratoryConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseVirtualMachines",
                table: "CourseVirtualMachines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSoftwareVersions",
                table: "CourseSoftwareVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSoftwares",
                table: "CourseSoftwares");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSaaSProducts",
                table: "CourseSaaSProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoursePhysicalServers",
                table: "CoursePhysicalServers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoursePersonnels",
                table: "CoursePersonnels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseLaboratories",
                table: "CourseLaboratories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseEquipmentModels",
                table: "CourseEquipmentModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseConfigurations",
                table: "CourseConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfigurationOSes",
                table: "ConfigurationOSes");

            migrationBuilder.RenameTable(
                name: "LaboratoryConfigurations",
                newName: "LaboratoryConfiguration");

            migrationBuilder.RenameTable(
                name: "CourseVirtualMachines",
                newName: "CourseVirtualMachine");

            migrationBuilder.RenameTable(
                name: "CourseSoftwareVersions",
                newName: "CourseSoftwareVersion");

            migrationBuilder.RenameTable(
                name: "CourseSoftwares",
                newName: "CourseSoftware");

            migrationBuilder.RenameTable(
                name: "CourseSaaSProducts",
                newName: "CourseSaaSProduct");

            migrationBuilder.RenameTable(
                name: "CoursePhysicalServers",
                newName: "CoursePhysicalServer");

            migrationBuilder.RenameTable(
                name: "CoursePersonnels",
                newName: "CoursePersonnel");

            migrationBuilder.RenameTable(
                name: "CourseLaboratories",
                newName: "CourseLaboratory");

            migrationBuilder.RenameTable(
                name: "CourseEquipmentModels",
                newName: "CourseEquipmentModel");

            migrationBuilder.RenameTable(
                name: "CourseConfigurations",
                newName: "CourseConfiguration");

            migrationBuilder.RenameTable(
                name: "ConfigurationOSes",
                newName: "ConfigurationOS");

            migrationBuilder.RenameIndex(
                name: "IX_LaboratoryConfigurations_ConfigurationId",
                table: "LaboratoryConfiguration",
                newName: "IX_LaboratoryConfiguration_ConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVirtualMachines_VirtualMachineId",
                table: "CourseVirtualMachine",
                newName: "IX_CourseVirtualMachine_VirtualMachineId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSoftwareVersions_SoftwareVersionId",
                table: "CourseSoftwareVersion",
                newName: "IX_CourseSoftwareVersion_SoftwareVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSoftwares_SoftwareId",
                table: "CourseSoftware",
                newName: "IX_CourseSoftware_SoftwareId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSaaSProducts_SaaSProductId",
                table: "CourseSaaSProduct",
                newName: "IX_CourseSaaSProduct_SaaSProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePhysicalServers_PhysicalServerId",
                table: "CoursePhysicalServer",
                newName: "IX_CoursePhysicalServer_PhysicalServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePersonnels_PersonnelId",
                table: "CoursePersonnel",
                newName: "IX_CoursePersonnel_PersonnelId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseLaboratories_LaboratoryId",
                table: "CourseLaboratory",
                newName: "IX_CourseLaboratory_LaboratoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseEquipmentModels_EquipmentModelId",
                table: "CourseEquipmentModel",
                newName: "IX_CourseEquipmentModel_EquipmentModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseConfigurations_ConfigurationId",
                table: "CourseConfiguration",
                newName: "IX_CourseConfiguration_ConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationOSes_OSId",
                table: "ConfigurationOS",
                newName: "IX_ConfigurationOS_OSId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LaboratoryConfiguration",
                table: "LaboratoryConfiguration",
                columns: new[] { "LaboratoryId", "ConfigurationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseVirtualMachine",
                table: "CourseVirtualMachine",
                columns: new[] { "CourseId", "VirtualMachineId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSoftwareVersion",
                table: "CourseSoftwareVersion",
                columns: new[] { "CourseId", "SoftwareVersionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSoftware",
                table: "CourseSoftware",
                columns: new[] { "CourseId", "SoftwareId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSaaSProduct",
                table: "CourseSaaSProduct",
                columns: new[] { "CourseId", "SaaSProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoursePhysicalServer",
                table: "CoursePhysicalServer",
                columns: new[] { "CourseId", "PhysicalServerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoursePersonnel",
                table: "CoursePersonnel",
                columns: new[] { "CourseId", "PersonnelId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseLaboratory",
                table: "CourseLaboratory",
                columns: new[] { "CourseId", "LaboratoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseEquipmentModel",
                table: "CourseEquipmentModel",
                columns: new[] { "CourseId", "EquipmentModelId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseConfiguration",
                table: "CourseConfiguration",
                columns: new[] { "CourseId", "ConfigurationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfigurationOS",
                table: "ConfigurationOS",
                columns: new[] { "ConfigurationId", "OSId" });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonnelId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Permission = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.Permission });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonnelId",
                table: "Users",
                column: "PersonnelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationOS_Configurations_ConfigurationId",
                table: "ConfigurationOS",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationOS_OperatingSystems_OSId",
                table: "ConfigurationOS",
                column: "OSId",
                principalTable: "OperatingSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseConfiguration_Configurations_ConfigurationId",
                table: "CourseConfiguration",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseConfiguration_Courses_CourseId",
                table: "CourseConfiguration",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEquipmentModel_Courses_CourseId",
                table: "CourseEquipmentModel",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEquipmentModel_EquipmentModels_EquipmentModelId",
                table: "CourseEquipmentModel",
                column: "EquipmentModelId",
                principalTable: "EquipmentModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLaboratory_Courses_CourseId",
                table: "CourseLaboratory",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLaboratory_Laboratories_LaboratoryId",
                table: "CourseLaboratory",
                column: "LaboratoryId",
                principalTable: "Laboratories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePersonnel_Courses_CourseId",
                table: "CoursePersonnel",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePersonnel_Personnel_PersonnelId",
                table: "CoursePersonnel",
                column: "PersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePhysicalServer_Courses_CourseId",
                table: "CoursePhysicalServer",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePhysicalServer_PhysicalServers_PhysicalServerId",
                table: "CoursePhysicalServer",
                column: "PhysicalServerId",
                principalTable: "PhysicalServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSaaSProduct_Courses_CourseId",
                table: "CourseSaaSProduct",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSaaSProduct_SaaSProducts_SaaSProductId",
                table: "CourseSaaSProduct",
                column: "SaaSProductId",
                principalTable: "SaaSProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftware_Courses_CourseId",
                table: "CourseSoftware",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftware_Softwares_SoftwareId",
                table: "CourseSoftware",
                column: "SoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwareVersion_Courses_CourseId",
                table: "CourseSoftwareVersion",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSoftwareVersion_SoftwareVersions_SoftwareVersionId",
                table: "CourseSoftwareVersion",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachine_Courses_CourseId",
                table: "CourseVirtualMachine",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVirtualMachine_VirtualMachines_VirtualMachineId",
                table: "CourseVirtualMachine",
                column: "VirtualMachineId",
                principalTable: "VirtualMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LaboratoryConfiguration_Configurations_ConfigurationId",
                table: "LaboratoryConfiguration",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LaboratoryConfiguration_Laboratories_LaboratoryId",
                table: "LaboratoryConfiguration",
                column: "LaboratoryId",
                principalTable: "Laboratories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_CreatedByUserId",
                table: "Sessions",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingNeeds_Users_ReviewedByUserId",
                table: "TeachingNeeds",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
