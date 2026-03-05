using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class InventoryModuleV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId",
                table: "CatalogImage");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogItem_CatalogCategory_CategoryId",
                table: "CatalogItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOption_CatalogItem_CatalogItemId",
                table: "CatalogOption");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId",
                table: "CatalogOptionValue");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogVariant_CatalogItem_CatalogItemId",
                table: "CatalogVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovement_CatalogVariant_CatalogVariantId",
                table: "InventoryMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Supplier_SupplierId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItem_Purchase_PurchaseId",
                table: "PurchaseItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseItem",
                table: "PurchaseItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_Purchase_PurchaseDate",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryMovement",
                table: "InventoryMovement");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovement_CreatedAt",
                table: "InventoryMovement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogVariant",
                table: "CatalogVariant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogOptionValue",
                table: "CatalogOptionValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogOption",
                table: "CatalogOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogItem",
                table: "CatalogItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogImage",
                table: "CatalogImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogCategory",
                table: "CatalogCategory");

            migrationBuilder.RenameTable(
                name: "Supplier",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "PurchaseItem",
                newName: "PurchaseItems");

            migrationBuilder.RenameTable(
                name: "Purchase",
                newName: "Purchases");

            migrationBuilder.RenameTable(
                name: "InventoryMovement",
                newName: "InventoryMovements");

            migrationBuilder.RenameTable(
                name: "CatalogVariant",
                newName: "CatalogVariants");

            migrationBuilder.RenameTable(
                name: "CatalogOptionValue",
                newName: "CatalogOptionValues");

            migrationBuilder.RenameTable(
                name: "CatalogOption",
                newName: "CatalogOptions");

            migrationBuilder.RenameTable(
                name: "CatalogItem",
                newName: "CatalogItems");

            migrationBuilder.RenameTable(
                name: "CatalogImage",
                newName: "CatalogImages");

            migrationBuilder.RenameTable(
                name: "CatalogCategory",
                newName: "CatalogCategories");

            migrationBuilder.RenameIndex(
                name: "IX_Supplier_BusinessId",
                table: "Suppliers",
                newName: "IX_Suppliers_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItems",
                newName: "IX_PurchaseItems_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_SupplierId",
                table: "Purchases",
                newName: "IX_Purchases_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_BusinessId",
                table: "Purchases",
                newName: "IX_Purchases_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovement_CatalogVariantId",
                table: "InventoryMovements",
                newName: "IX_InventoryMovements_CatalogVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovement_BusinessId_CreatedAt",
                table: "InventoryMovements",
                newName: "IX_InventoryMovements_BusinessId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovement_BusinessId",
                table: "InventoryMovements",
                newName: "IX_InventoryMovements_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogVariant_SKU",
                table: "CatalogVariants",
                newName: "IX_CatalogVariants_SKU");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogVariant_CatalogItemId",
                table: "CatalogVariants",
                newName: "IX_CatalogVariants_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogOptionValue_CatalogOptionId",
                table: "CatalogOptionValues",
                newName: "IX_CatalogOptionValues_CatalogOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogOption_CatalogItemId",
                table: "CatalogOptions",
                newName: "IX_CatalogOptions_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItem_CategoryId",
                table: "CatalogItems",
                newName: "IX_CatalogItems_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItem_BusinessId_IsActive",
                table: "CatalogItems",
                newName: "IX_CatalogItems_BusinessId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItem_BusinessId_CategoryId",
                table: "CatalogItems",
                newName: "IX_CatalogItems_BusinessId_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItem_BusinessId",
                table: "CatalogItems",
                newName: "IX_CatalogItems_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogImage_CatalogItemId",
                table: "CatalogImages",
                newName: "IX_CatalogImages_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogCategory_BusinessId",
                table: "CatalogCategories",
                newName: "IX_CatalogCategories_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseItems",
                table: "PurchaseItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryMovements",
                table: "InventoryMovements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogVariants",
                table: "CatalogVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogOptionValues",
                table: "CatalogOptionValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogOptions",
                table: "CatalogOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogItems",
                table: "CatalogItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogImages",
                table: "CatalogImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogCategories",
                table: "CatalogCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogImages_CatalogItems_CatalogItemId",
                table: "CatalogImages",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogItems_CatalogCategories_CategoryId",
                table: "CatalogItems",
                column: "CategoryId",
                principalTable: "CatalogCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogOptions_CatalogItems_CatalogItemId",
                table: "CatalogOptions",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogOptionValues_CatalogOptions_CatalogOptionId",
                table: "CatalogOptionValues",
                column: "CatalogOptionId",
                principalTable: "CatalogOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogVariants_CatalogItems_CatalogItemId",
                table: "CatalogVariants",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_CatalogVariants_CatalogVariantId",
                table: "InventoryMovements",
                column: "CatalogVariantId",
                principalTable: "CatalogVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Purchases_PurchaseId",
                table: "PurchaseItems",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Suppliers_SupplierId",
                table: "Purchases",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogImages_CatalogItems_CatalogItemId",
                table: "CatalogImages");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogItems_CatalogCategories_CategoryId",
                table: "CatalogItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOptions_CatalogItems_CatalogItemId",
                table: "CatalogOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOptionValues_CatalogOptions_CatalogOptionId",
                table: "CatalogOptionValues");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogVariants_CatalogItems_CatalogItemId",
                table: "CatalogVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_CatalogVariants_CatalogVariantId",
                table: "InventoryMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Purchases_PurchaseId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Suppliers_SupplierId",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseItems",
                table: "PurchaseItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryMovements",
                table: "InventoryMovements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogVariants",
                table: "CatalogVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogOptionValues",
                table: "CatalogOptionValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogOptions",
                table: "CatalogOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogItems",
                table: "CatalogItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogImages",
                table: "CatalogImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatalogCategories",
                table: "CatalogCategories");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Supplier");

            migrationBuilder.RenameTable(
                name: "Purchases",
                newName: "Purchase");

            migrationBuilder.RenameTable(
                name: "PurchaseItems",
                newName: "PurchaseItem");

            migrationBuilder.RenameTable(
                name: "InventoryMovements",
                newName: "InventoryMovement");

            migrationBuilder.RenameTable(
                name: "CatalogVariants",
                newName: "CatalogVariant");

            migrationBuilder.RenameTable(
                name: "CatalogOptionValues",
                newName: "CatalogOptionValue");

            migrationBuilder.RenameTable(
                name: "CatalogOptions",
                newName: "CatalogOption");

            migrationBuilder.RenameTable(
                name: "CatalogItems",
                newName: "CatalogItem");

            migrationBuilder.RenameTable(
                name: "CatalogImages",
                newName: "CatalogImage");

            migrationBuilder.RenameTable(
                name: "CatalogCategories",
                newName: "CatalogCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Supplier",
                newName: "IX_Supplier_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_SupplierId",
                table: "Purchase",
                newName: "IX_Purchase_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_BusinessId",
                table: "Purchase",
                newName: "IX_Purchase_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItems_PurchaseId",
                table: "PurchaseItem",
                newName: "IX_PurchaseItem_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovements_CatalogVariantId",
                table: "InventoryMovement",
                newName: "IX_InventoryMovement_CatalogVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovements_BusinessId_CreatedAt",
                table: "InventoryMovement",
                newName: "IX_InventoryMovement_BusinessId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryMovements_BusinessId",
                table: "InventoryMovement",
                newName: "IX_InventoryMovement_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogVariants_SKU",
                table: "CatalogVariant",
                newName: "IX_CatalogVariant_SKU");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogVariants_CatalogItemId",
                table: "CatalogVariant",
                newName: "IX_CatalogVariant_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogOptionValues_CatalogOptionId",
                table: "CatalogOptionValue",
                newName: "IX_CatalogOptionValue_CatalogOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogOptions_CatalogItemId",
                table: "CatalogOption",
                newName: "IX_CatalogOption_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItems_CategoryId",
                table: "CatalogItem",
                newName: "IX_CatalogItem_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItems_BusinessId_IsActive",
                table: "CatalogItem",
                newName: "IX_CatalogItem_BusinessId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItems_BusinessId_CategoryId",
                table: "CatalogItem",
                newName: "IX_CatalogItem_BusinessId_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogItems_BusinessId",
                table: "CatalogItem",
                newName: "IX_CatalogItem_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogImages_CatalogItemId",
                table: "CatalogImage",
                newName: "IX_CatalogImage_CatalogItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CatalogCategories_BusinessId",
                table: "CatalogCategory",
                newName: "IX_CatalogCategory_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseItem",
                table: "PurchaseItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryMovement",
                table: "InventoryMovement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogVariant",
                table: "CatalogVariant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogOptionValue",
                table: "CatalogOptionValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogOption",
                table: "CatalogOption",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogItem",
                table: "CatalogItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogImage",
                table: "CatalogImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatalogCategory",
                table: "CatalogCategory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_PurchaseDate",
                table: "Purchase",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovement_CreatedAt",
                table: "InventoryMovement",
                column: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId",
                table: "CatalogImage",
                column: "CatalogItemId",
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
                name: "FK_CatalogOption_CatalogItem_CatalogItemId",
                table: "CatalogOption",
                column: "CatalogItemId",
                principalTable: "CatalogItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId",
                table: "CatalogOptionValue",
                column: "CatalogOptionId",
                principalTable: "CatalogOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogVariant_CatalogItem_CatalogItemId",
                table: "CatalogVariant",
                column: "CatalogItemId",
                principalTable: "CatalogItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_PurchaseItem_Purchase_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId",
                principalTable: "Purchase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
