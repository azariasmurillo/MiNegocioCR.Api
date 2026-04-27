using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class SaleHybridBillingFieldsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogVariantId",
                table: "SaleItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SaleItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_CatalogVariantId",
                table: "SaleItems",
                column: "CatalogVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_CatalogVariants_CatalogVariantId",
                table: "SaleItems",
                column: "CatalogVariantId",
                principalTable: "CatalogVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_CatalogVariants_CatalogVariantId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_CatalogVariantId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SaleItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogVariantId",
                table: "SaleItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
