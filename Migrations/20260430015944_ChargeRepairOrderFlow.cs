using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChargeRepairOrderFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvoicedAt",
                table: "RepairOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvoiced",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SaleId",
                table: "RepairOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BusinessId_RepairOrderId",
                table: "Sales",
                columns: new[] { "BusinessId", "RepairOrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_BusinessId_RepairOrderId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoicedAt",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "IsInvoiced",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "SaleId",
                table: "RepairOrders");
        }
    }
}
