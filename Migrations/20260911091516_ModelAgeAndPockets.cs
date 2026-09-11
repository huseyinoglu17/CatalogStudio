using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogStudio.Migrations
{
    /// <inheritdoc />
    public partial class ModelAgeAndPockets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPockets",
                table: "Catalogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelAge",
                table: "Catalogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelAgeUnit",
                table: "Catalogs",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPockets",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "ModelAge",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "ModelAgeUnit",
                table: "Catalogs");
        }
    }
}
