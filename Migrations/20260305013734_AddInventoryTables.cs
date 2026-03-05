using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    HasVariants = table.Column<bool>(type: "boolean", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TrackStock = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItem_CatalogCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CatalogCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchase_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogImage_CatalogItem_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogVariant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SKU = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogVariant_CatalogItem_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_Timestamp",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImage_CatalogItemId",
                table: "CatalogImage",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_BusinessId",
                table: "CatalogItem",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_BusinessId_CategoryId",
                table: "CatalogItem",
                columns: new[] { "BusinessId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_BusinessId_IsActive",
                table: "CatalogItem",
                columns: new[] { "BusinessId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_CategoryId",
                table: "CatalogItem",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariant_CatalogItemId",
                table: "CatalogVariant",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariant_SKU",
                table: "CatalogVariant",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovement_BusinessId",
                table: "InventoryMovement",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovement_BusinessId_CreatedAt",
                table: "InventoryMovement",
                columns: new[] { "BusinessId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovement_CatalogVariantId",
                table: "InventoryMovement",
                column: "CatalogVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovement_CreatedAt",
                table: "InventoryMovement",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_BusinessId",
                table: "Purchase",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_PurchaseDate",
                table: "Purchase",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_SupplierId",
                table: "Purchase",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogImage");

            migrationBuilder.DropTable(
                name: "CatalogVariant");

            migrationBuilder.DropTable(
                name: "InventoryMovement");

            migrationBuilder.DropTable(
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "CatalogItem");

            migrationBuilder.DropTable(
                name: "Purchase");

            migrationBuilder.DropTable(
                name: "CatalogCategory");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_Timestamp",
                table: "WhatsAppMessages");
        }
    }
}
