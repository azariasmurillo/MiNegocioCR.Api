using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogItem_CatalogCategory_CategoryId",
                table: "CatalogItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Supplier_SupplierId",
                table: "Purchase");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseId1",
                table: "PurchaseItem",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId1",
                table: "Purchase",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogItemId1",
                table: "CatalogImage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CatalogOption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CatalogItemId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogOption_CatalogItem_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogOption_CatalogItem_CatalogItemId1",
                        column: x => x.CatalogItemId1,
                        principalTable: "CatalogItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogOptionValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CatalogOptionId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogOptionValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId",
                        column: x => x.CatalogOptionId,
                        principalTable: "CatalogOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId1",
                        column: x => x.CatalogOptionId1,
                        principalTable: "CatalogOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_BusinessId",
                table: "Supplier",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId1",
                table: "PurchaseItem",
                column: "PurchaseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_SupplierId1",
                table: "Purchase",
                column: "SupplierId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImage_CatalogItemId1",
                table: "CatalogImage",
                column: "CatalogItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategory_BusinessId",
                table: "CatalogCategory",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOption_CatalogItemId",
                table: "CatalogOption",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOption_CatalogItemId1",
                table: "CatalogOption",
                column: "CatalogItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptionValue_CatalogOptionId",
                table: "CatalogOptionValue",
                column: "CatalogOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptionValue_CatalogOptionId1",
                table: "CatalogOptionValue",
                column: "CatalogOptionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId1",
                table: "CatalogImage",
                column: "CatalogItemId1",
                principalTable: "CatalogItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogItem_CatalogCategory_CategoryId",
                table: "CatalogItem",
                column: "CategoryId",
                principalTable: "CatalogCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovement_CatalogVariant_CatalogVariantId",
                table: "InventoryMovement",
                column: "CatalogVariantId",
                principalTable: "CatalogVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Supplier_SupplierId",
                table: "Purchase",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Supplier_SupplierId1",
                table: "Purchase",
                column: "SupplierId1",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItem_Purchase_PurchaseId1",
                table: "PurchaseItem",
                column: "PurchaseId1",
                principalTable: "Purchase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId1",
                table: "CatalogImage");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogItem_CatalogCategory_CategoryId",
                table: "CatalogItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovement_CatalogVariant_CatalogVariantId",
                table: "InventoryMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Supplier_SupplierId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Supplier_SupplierId1",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItem_Purchase_PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "CatalogOptionValue");

            migrationBuilder.DropTable(
                name: "CatalogOption");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_BusinessId",
                table: "Supplier");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItem_PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropIndex(
                name: "IX_Purchase_SupplierId1",
                table: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_CatalogImage_CatalogItemId1",
                table: "CatalogImage");

            migrationBuilder.DropIndex(
                name: "IX_CatalogCategory_BusinessId",
                table: "CatalogCategory");

            migrationBuilder.DropColumn(
                name: "PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "SupplierId1",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "CatalogItemId1",
                table: "CatalogImage");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogItem_CatalogCategory_CategoryId",
                table: "CatalogItem",
                column: "CategoryId",
                principalTable: "CatalogCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Supplier_SupplierId",
                table: "Purchase",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
