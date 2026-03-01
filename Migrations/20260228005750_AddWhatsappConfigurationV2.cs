using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsappConfigurationV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsappDisplayPhoneNumber",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhatsappTokenExpiresAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappDisplayPhoneNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WhatsappTokenExpiresAt",
                table: "Businesses");
        }
    }
}
