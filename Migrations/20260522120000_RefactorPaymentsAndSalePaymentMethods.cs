using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPaymentsAndSalePaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla SalePaymentMethods (reemplaza booleans PayCash/etc en Sales)
            migrationBuilder.CreateTable(
                name: "SalePaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalePaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalePaymentMethods_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalePaymentMethods_SaleId",
                table: "SalePaymentMethods",
                column: "SaleId");

            // 2. Agregar TotalOrden y PrepaidAmount a Sales
            migrationBuilder.AddColumn<decimal>(
                name: "TotalOrden",
                table: "Sales",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaidAmount",
                table: "Sales",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Inicializar TotalOrden con Total para registros existentes
            migrationBuilder.Sql(
                "UPDATE \"Sales\" SET \"TotalOrden\" = \"Total\", \"PrepaidAmount\" = 0;");

            // 3. Eliminar booleans de pago de Sales (reemplazados por SalePaymentMethods)
            migrationBuilder.DropColumn(name: "PayCash",     table: "Sales");
            migrationBuilder.DropColumn(name: "PayTransfer", table: "Sales");
            migrationBuilder.DropColumn(name: "PaySinpe",    table: "Sales");
            migrationBuilder.DropColumn(name: "PayCard",     table: "Sales");

            // 4. Eliminar booleans de pago de RepairOrders (el método se define al facturar)
            migrationBuilder.DropColumn(name: "PayCash",     table: "RepairOrders");
            migrationBuilder.DropColumn(name: "PayTransfer", table: "RepairOrders");
            migrationBuilder.DropColumn(name: "PaySinpe",    table: "RepairOrders");
            migrationBuilder.DropColumn(name: "PayCard",     table: "RepairOrders");

            // 5. Agregar Reference a Payments (número SINPE, código transferencia, etc.)
            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SalePaymentMethods");

            migrationBuilder.DropColumn(name: "TotalOrden",     table: "Sales");
            migrationBuilder.DropColumn(name: "PrepaidAmount",  table: "Sales");

            migrationBuilder.AddColumn<bool>(name: "PayCash",     table: "Sales",        nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PayTransfer", table: "Sales",        nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PaySinpe",    table: "Sales",        nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PayCard",     table: "Sales",        nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PayCash",     table: "RepairOrders", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PayTransfer", table: "RepairOrders", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PaySinpe",    table: "RepairOrders", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "PayCard",     table: "RepairOrders", nullable: false, defaultValue: false);

            migrationBuilder.DropColumn(name: "Reference", table: "Payments");
        }
    }
}
