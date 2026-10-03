using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogStudio.Migrations
{
    /// <inheritdoc />
    public partial class EmbroideryPricingAndSeparateSizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorTokenPrice",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 75);

            migrationBuilder.AddColumn<int>(
                name: "DecorationType",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DetailImagePath",
                table: "Catalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GenerationInstructions",
                table: "Catalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegenerationReason",
                table: "Catalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WhiteOutputHeight",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1100);

            migrationBuilder.AddColumn<int>(
                name: "WhiteOutputWidth",
                table: "Catalogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1400);

            // Previously both output types used the same saved dimensions.
            migrationBuilder.Sql("UPDATE Catalogs SET WhiteOutputWidth = OutputWidth, WhiteOutputHeight = OutputHeight");

            migrationBuilder.CreateTable(
                name: "TokenPricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ColorTokenPrice = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPricing", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenPricing");

            migrationBuilder.DropColumn(
                name: "ColorTokenPrice",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "DecorationType",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "DetailImagePath",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "GenerationInstructions",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "RegenerationReason",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "WhiteOutputHeight",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "WhiteOutputWidth",
                table: "Catalogs");
        }
    }
}
