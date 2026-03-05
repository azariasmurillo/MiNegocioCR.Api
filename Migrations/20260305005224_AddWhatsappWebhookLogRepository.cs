using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsappWebhookLogRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders");

            migrationBuilder.CreateTable(
                name: "WhatsAppConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    LastMessage = table.Column<string>(type: "text", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnreadCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsappWebhookLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsappWebhookLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId",
                table: "RepairOrders",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_BusinessId_PhoneNumber",
                table: "WhatsAppConversations",
                columns: new[] { "BusinessId", "PhoneNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppConversations");

            migrationBuilder.DropTable(
                name: "WhatsappWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_BusinessId",
                table: "RepairOrders");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BusinessId_OrderNumber",
                table: "RepairOrders",
                columns: new[] { "BusinessId", "OrderNumber" },
                unique: true);
        }
    }
}
