using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIndicesVersion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId1",
                table: "CatalogImage");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOption_CatalogItem_CatalogItemId1",
                table: "CatalogOption");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId1",
                table: "CatalogOptionValue");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Supplier_SupplierId1",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItem_Purchase_PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItem_PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropIndex(
                name: "IX_Purchase_SupplierId1",
                table: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_CatalogOptionValue_CatalogOptionId1",
                table: "CatalogOptionValue");

            migrationBuilder.DropIndex(
                name: "IX_CatalogOption_CatalogItemId1",
                table: "CatalogOption");

            migrationBuilder.DropIndex(
                name: "IX_CatalogImage_CatalogItemId1",
                table: "CatalogImage");

            migrationBuilder.DropColumn(
                name: "PurchaseId1",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "SupplierId1",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "CatalogOptionId1",
                table: "CatalogOptionValue");

            migrationBuilder.DropColumn(
                name: "CatalogItemId1",
                table: "CatalogOption");

            migrationBuilder.DropColumn(
                name: "CatalogItemId1",
                table: "CatalogImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "CatalogOptionId1",
                table: "CatalogOptionValue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogItemId1",
                table: "CatalogOption",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogItemId1",
                table: "CatalogImage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId1",
                table: "PurchaseItem",
                column: "PurchaseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_SupplierId1",
                table: "Purchase",
                column: "SupplierId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptionValue_CatalogOptionId1",
                table: "CatalogOptionValue",
                column: "CatalogOptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOption_CatalogItemId1",
                table: "CatalogOption",
                column: "CatalogItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImage_CatalogItemId1",
                table: "CatalogImage",
                column: "CatalogItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogImage_CatalogItem_CatalogItemId1",
                table: "CatalogImage",
                column: "CatalogItemId1",
                principalTable: "CatalogItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogOption_CatalogItem_CatalogItemId1",
                table: "CatalogOption",
                column: "CatalogItemId1",
                principalTable: "CatalogItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogOptionValue_CatalogOption_CatalogOptionId1",
                table: "CatalogOptionValue",
                column: "CatalogOptionId1",
                principalTable: "CatalogOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
