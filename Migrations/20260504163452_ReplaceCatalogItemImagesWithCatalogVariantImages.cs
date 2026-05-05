using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCatalogItemImagesWithCatalogVariantImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItemImages");

            migrationBuilder.CreateTable(
                name: "CatalogVariantImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogVariantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogVariantImages_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogVariantImages_CatalogVariants_CatalogVariantId",
                        column: x => x.CatalogVariantId,
                        principalTable: "CatalogVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantImages_BusinessId",
                table: "CatalogVariantImages",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantImages_BusinessId_CatalogVariantId",
                table: "CatalogVariantImages",
                columns: new[] { "BusinessId", "CatalogVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantImages_CatalogVariantId",
                table: "CatalogVariantImages",
                column: "CatalogVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogVariantImages");

            migrationBuilder.CreateTable(
                name: "CatalogItemImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItemImages_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogItemImages_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemImages_BusinessId",
                table: "CatalogItemImages",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemImages_BusinessId_CatalogItemId",
                table: "CatalogItemImages",
                columns: new[] { "BusinessId", "CatalogItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemImages_CatalogItemId",
                table: "CatalogItemImages",
                column: "CatalogItemId");
        }
    }
}
