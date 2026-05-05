using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public class AddSaleCostAndProfitMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "SaleItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "Sales",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalProfit",
                table: "Sales",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleItems_CostPrice_NonNegative",
                table: "SaleItems",
                sql: "\"CostPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sales_TotalCost_NonNegative",
                table: "Sales",
                sql: "\"TotalCost\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sales_TotalCost_NonNegative",
                table: "Sales");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleItems_CostPrice_NonNegative",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "TotalProfit",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "SaleItems");
        }
    }
}
