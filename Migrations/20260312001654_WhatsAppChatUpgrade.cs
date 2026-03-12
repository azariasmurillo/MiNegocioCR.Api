using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class WhatsAppChatUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentType",
                table: "WhatsAppMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "WhatsAppMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastMessageAt",
                table: "WhatsAppConversations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "LastMessage",
                table: "WhatsAppConversations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "WhatsAppConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "WhatsAppConversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RepairOrderId",
                table: "WhatsAppConversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WhatsAppConversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_IsArchived",
                table: "WhatsAppConversations",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_LastMessageAt",
                table: "WhatsAppConversations",
                column: "LastMessageAt");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversations_RepairOrders_RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_IsArchived",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_LastMessageAt",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "RepairOrderId",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WhatsAppConversations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastMessageAt",
                table: "WhatsAppConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastMessage",
                table: "WhatsAppConversations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
