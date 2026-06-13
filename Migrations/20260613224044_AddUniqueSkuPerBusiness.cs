using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSkuPerBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "CatalogVariants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkuNormalized",
                table: "CatalogVariants",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "CatalogVariants" AS v
                SET "BusinessId" = ci."BusinessId",
                    "SkuNormalized" = LOWER(TRIM(v."SKU"))
                FROM "CatalogItems" AS ci
                WHERE ci."Id" = v."CatalogItemId";
                """);

            migrationBuilder.Sql(
                """
                UPDATE "CatalogVariants"
                SET "SkuNormalized" = NULL
                WHERE "SKU" IS NULL OR TRIM("SKU") = '';
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessId",
                table: "CatalogVariants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariants_BusinessId",
                table: "CatalogVariants",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariants_BusinessId_SkuNormalized",
                table: "CatalogVariants",
                columns: new[] { "BusinessId", "SkuNormalized" },
                unique: true,
                filter: "\"SkuNormalized\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogVariants_Businesses_BusinessId",
                table: "CatalogVariants",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogVariants_Businesses_BusinessId",
                table: "CatalogVariants");

            migrationBuilder.DropIndex(
                name: "IX_CatalogVariants_BusinessId",
                table: "CatalogVariants");

            migrationBuilder.DropIndex(
                name: "IX_CatalogVariants_BusinessId_SkuNormalized",
                table: "CatalogVariants");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "CatalogVariants");

            migrationBuilder.DropColumn(
                name: "SkuNormalized",
                table: "CatalogVariants");
        }
    }
}
