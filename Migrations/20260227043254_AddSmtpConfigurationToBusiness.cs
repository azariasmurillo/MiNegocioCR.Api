using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSmtpConfigurationToBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromName",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Businesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "Businesses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SmtpFromName",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "Businesses");
        }
    }
}
