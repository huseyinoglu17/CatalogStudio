using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogStudio.Migrations
{
    /// <inheritdoc />
    public partial class PreserveRegenerationCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegenerationChanges",
                table: "Catalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegenerationChanges",
                table: "Catalogs");
        }
    }
}
