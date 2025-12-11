using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onboarding.Tool.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSetupStepTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstanceSetupStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetupStep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InstanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceSetupStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanceSetupStep_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstanceSetupStep_InstanceId",
                table: "InstanceSetupStep",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceSetupStep_SetupStep_InstanceId",
                table: "InstanceSetupStep",
                columns: new[] { "SetupStep", "InstanceId" },
                unique: true,
                filter: "[InstanceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstanceSetupStep");
        }
    }
}
