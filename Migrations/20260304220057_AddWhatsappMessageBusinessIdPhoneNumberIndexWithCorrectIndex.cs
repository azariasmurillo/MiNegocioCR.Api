using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsappMessageBusinessIdPhoneNumberIndexWithCorrectIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsappMessages_Businesses_BusinessId",
                table: "WhatsappMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsappMessages",
                table: "WhatsappMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappMessages_BusinessId",
                table: "WhatsappMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappMessages_MessageId",
                table: "WhatsappMessages");

            migrationBuilder.RenameTable(
                name: "WhatsappMessages",
                newName: "WhatsAppMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsAppMessages",
                table: "WhatsAppMessages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber",
                table: "WhatsAppMessages",
                columns: new[] { "BusinessId", "PhoneNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Businesses_BusinessId",
                table: "WhatsAppMessages",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Businesses_BusinessId",
                table: "WhatsAppMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsAppMessages",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_BusinessId_PhoneNumber",
                table: "WhatsAppMessages");

            migrationBuilder.RenameTable(
                name: "WhatsAppMessages",
                newName: "WhatsappMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsappMessages",
                table: "WhatsappMessages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappMessages_BusinessId",
                table: "WhatsappMessages",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappMessages_MessageId",
                table: "WhatsappMessages",
                column: "MessageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsappMessages_Businesses_BusinessId",
                table: "WhatsappMessages",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
