using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class SaleHybridBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HaciendaConsecutive",
                table: "Sales",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Sales",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RepairOrderId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "SaleItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Product");

            migrationBuilder.Sql(
                """
                WITH numbered AS (
                    SELECT s."Id",
                           s."BusinessId",
                           s."SaleDate",
                           ROW_NUMBER() OVER (
                               PARTITION BY s."BusinessId", date_trunc('day', s."SaleDate")
                               ORDER BY s."SaleDate", s."Id"
                           ) AS rn
                    FROM "Sales" s
                )
                UPDATE "Sales" s
                SET "InvoiceNumber" =
                    'FACT-' || to_char(n."SaleDate", 'YYYYMMDD') || '-' || lpad(n.rn::text, 4, '0')
                FROM numbered n
                WHERE s."Id" = n."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "Sales",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BusinessId_InvoiceNumber",
                table: "Sales",
                columns: new[] { "BusinessId", "InvoiceNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_BusinessId_InvoiceNumber",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "HaciendaConsecutive",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "RepairOrderId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "SaleItems");
        }
    }
}
