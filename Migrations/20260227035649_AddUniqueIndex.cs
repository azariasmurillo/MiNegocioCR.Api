using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId",
                table: "RepairOrders");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders",
                columns: new[] { "BusinessId", "OrderNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId",
                table: "RepairOrders",
                column: "BusinessId");
        }
    }
}
