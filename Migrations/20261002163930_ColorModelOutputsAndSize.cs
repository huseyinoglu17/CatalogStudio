using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogStudio.Migrations
{
    /// <inheritdoc />
    public partial class ColorModelOutputsAndSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OutputHeight",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1100);

            migrationBuilder.AddColumn<int>(
                name: "OutputWidth",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1400);

            migrationBuilder.AddColumn<string>(
                name: "WhiteModelPathsJson",
                table: "Catalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputHeight",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "OutputWidth",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "WhiteModelPathsJson",
                table: "Catalogs");
        }
    }
}
