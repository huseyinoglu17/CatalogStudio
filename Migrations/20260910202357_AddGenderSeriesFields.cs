using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderSeriesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModelGender",
                table: "Catalogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeriesCount",
                table: "Catalogs",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelGender",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "SeriesCount",
                table: "Catalogs");
        }
    }
}
