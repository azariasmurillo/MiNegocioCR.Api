using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrderIsActiveAndStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId_Status",
                table: "RepairOrders",
                columns: new[] { "BusinessId", "Status" });

            // Órdenes ya canceladas (5): marcar inactivas para alinear con la nueva lógica.
            migrationBuilder.Sql("""UPDATE "RepairOrders" SET "IsActive" = false WHERE "Status" = 5;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId_Status",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RepairOrders");
        }
    }
}
