using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogOptionAndOptionValueIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CatalogVariantOptionValues_CatalogVariantId",
                table: "CatalogVariantOptionValues");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CatalogOptionValues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CatalogOptions",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CatalogOptionValues");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CatalogOptions");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantOptionValues_CatalogVariantId",
                table: "CatalogVariantOptionValues",
                column: "CatalogVariantId");
        }
    }
}
