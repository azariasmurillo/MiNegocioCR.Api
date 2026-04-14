using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWhatsappMessageBusinessId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Businesses_BusinessId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber_Timestamp",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "WhatsAppMessages");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ConversationId_Timestamp",
                table: "WhatsAppMessages",
                columns: new[] { "ConversationId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_ConversationId_Timestamp",
                table: "WhatsAppMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "WhatsAppMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber_Timestamp",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "PhoneNumber", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Businesses_BusinessId",
                table: "WhatsAppMessages",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
