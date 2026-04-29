using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrderBillingAndPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "RepairOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PayCard",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PayCash",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PaySinpe",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PayTransfer",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "PayCard",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "PayCash",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "PaySinpe",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "PayTransfer",
                table: "RepairOrders");
        }
    }
}
