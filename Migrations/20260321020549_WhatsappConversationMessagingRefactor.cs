using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class WhatsappConversationMessagingRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversations_RepairOrders_RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "WhatsAppMessages",
                type: "uuid",
                nullable: true);

            // Crear conversaciones faltantes para pares (BusinessId, PhoneNumber) que solo existen en mensajes
            migrationBuilder.Sql("""
                INSERT INTO "WhatsAppConversations" ("Id", "BusinessId", "PhoneNumber", "CustomerName", "LastMessage", "LastMessageAt", "UnreadCount", "Status", "IsArchived", "CreatedAt")
                SELECT gen_random_uuid(), m."BusinessId", m."PhoneNumber", NULL, NULL, NULL, 0, 0, false, NOW()
                FROM "WhatsAppMessages" m
                WHERE NOT EXISTS (
                    SELECT 1 FROM "WhatsAppConversations" c
                    WHERE c."BusinessId" = m."BusinessId" AND c."PhoneNumber" = m."PhoneNumber"
                )
                GROUP BY m."BusinessId", m."PhoneNumber";
                """);

            migrationBuilder.Sql("""
                UPDATE "WhatsAppMessages" AS m
                SET "ConversationId" = c."Id"
                FROM "WhatsAppConversations" AS c
                WHERE c."BusinessId" = m."BusinessId"
                  AND c."PhoneNumber" = m."PhoneNumber"
                  AND m."ConversationId" IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM "WhatsAppMessages" WHERE "ConversationId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "WhatsAppMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_BusinessId",
                table: "Contacts",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId",
                principalTable: "WhatsAppConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_BusinessId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "RepairOrderId",
                table: "WhatsAppConversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_RepairOrderId",
                table: "WhatsAppConversations",
                column: "RepairOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversations_RepairOrders_RepairOrderId",
                table: "WhatsAppConversations",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "Id");
        }
    }
}
