using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onboarding.Tool.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCardOverridesToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardHeightOverride",
                table: "Image",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardWidthOverride",
                table: "Image",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardXOverride",
                table: "Image",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardYOverride",
                table: "Image",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardHeightOverride",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "CardWidthOverride",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "CardXOverride",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "CardYOverride",
                table: "Image");
        }
    }
}
