using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onboarding.Tool.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DirectMailConfirmed",
                table: "Instances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LetterheadConfirmed",
                table: "Instances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TemplatesConfirmed",
                table: "Instances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DirectMailConfirmed",
                table: "Instances");

            migrationBuilder.DropColumn(
                name: "LetterheadConfirmed",
                table: "Instances");

            migrationBuilder.DropColumn(
                name: "TemplatesConfirmed",
                table: "Instances");
        }
    }
}
