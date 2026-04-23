using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrderItemsAndStringOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "RepairOrders" ALTER COLUMN "OrderNumber" TYPE text USING lpad("OrderNumber"::text, 6, '0');
                """);

            migrationBuilder.CreateTable(
                name: "RepairOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", maxLength: 2000, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrderItems_CatalogVariants_CatalogVariantId",
                        column: x => x.CatalogVariantId,
                        principalTable: "CatalogVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrderItems_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderItems_CatalogVariantId",
                table: "RepairOrderItems",
                column: "CatalogVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderItems_RepairOrderId",
                table: "RepairOrderItems",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId_CreatedAt",
                table: "RepairOrders",
                columns: new[] { "BusinessId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders",
                columns: new[] { "BusinessId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_CreatedAt",
                table: "RepairOrders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_CreatedAt",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId_CreatedAt",
                table: "RepairOrders");

            migrationBuilder.DropTable(
                name: "RepairOrderItems");

            migrationBuilder.Sql(
                """
                ALTER TABLE "RepairOrders" ALTER COLUMN "OrderNumber" TYPE integer USING ("OrderNumber")::integer;
                """);
        }
    }
}
