using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class CatalogVariantOptionValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogVariantOptionValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogOptionValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogVariantOptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogVariantOptionValues_CatalogOptionValues_CatalogOptionValueId",
                        column: x => x.CatalogOptionValueId,
                        principalTable: "CatalogOptionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogVariantOptionValues_CatalogVariants_CatalogVariantId",
                        column: x => x.CatalogVariantId,
                        principalTable: "CatalogVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantOptionValues_CatalogOptionValueId",
                table: "CatalogVariantOptionValues",
                column: "CatalogOptionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantOptionValues_CatalogVariantId",
                table: "CatalogVariantOptionValues",
                column: "CatalogVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantOptionValues_CatalogVariantId_CatalogOptionValueId",
                table: "CatalogVariantOptionValues",
                columns: new[] { "CatalogVariantId", "CatalogOptionValueId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogVariantOptionValues");
        }
    }
}
