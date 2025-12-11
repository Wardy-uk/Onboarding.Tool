using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onboarding.Tool.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Town = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PostCode1 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PostCode2 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SalesEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SalesPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LettingsEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LettingsPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    InstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Branches_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Height = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Image_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstanceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Setting = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanceSettings_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortalAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortalAccount_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchBuildDistricts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AllSectors = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchBuildDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchBuildDistricts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Setting = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchSetting_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchBuildDistrictSectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchBuildDistrictSectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchBuildDistrictSectors_BranchBuildDistricts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "BranchBuildDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchBuildDistricts_BranchId_District",
                table: "BranchBuildDistricts",
                columns: new[] { "BranchId", "District" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchBuildDistrictSectors_DistrictId_Sector",
                table: "BranchBuildDistrictSectors",
                columns: new[] { "DistrictId", "Sector" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_AddressId",
                table: "Branches",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_InstanceId",
                table: "Branches",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSetting_BranchId",
                table: "BranchSetting",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSetting_Setting_Value_BranchId",
                table: "BranchSetting",
                columns: new[] { "Setting", "Value", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_InstanceId",
                table: "Image",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_Type_InstanceId",
                table: "Image",
                columns: new[] { "Type", "InstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstanceSettings_InstanceId",
                table: "InstanceSettings",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceSettings_Setting_Value_InstanceId",
                table: "InstanceSettings",
                columns: new[] { "Setting", "Value", "InstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalAccount_InstanceId",
                table: "PortalAccount",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalAccount_PortalName_InstanceId",
                table: "PortalAccount",
                columns: new[] { "PortalName", "InstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstanceId",
                table: "Users",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_InstanceId",
                table: "Users",
                columns: new[] { "Username", "InstanceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchBuildDistrictSectors");

            migrationBuilder.DropTable(
                name: "BranchSetting");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "InstanceSettings");

            migrationBuilder.DropTable(
                name: "PortalAccount");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BranchBuildDistricts");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Instances");
        }
    }
}
