using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsappConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsappAccessToken",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappBusinessAccountId",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappPhoneNumberId",
                table: "Businesses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappAccessToken",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WhatsappBusinessAccountId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WhatsappPhoneNumberId",
                table: "Businesses");
        }
    }
}
