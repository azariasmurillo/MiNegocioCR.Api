using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class WhatsAppChatUpgradeVersion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_Timestamp",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "WhatsAppConversations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber_Timestamp",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "PhoneNumber", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages",
                column: "MessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber_Timestamp",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "WhatsAppConversations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_Timestamp",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_MessageId",
                table: "WhatsAppMessages",
                column: "MessageId");
        }
    }
}
