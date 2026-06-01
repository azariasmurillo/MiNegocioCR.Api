using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContactEmailCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMarketingEmailAt",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactEmailCampaignLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResendMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InactiveDaysUsed = table.Column<int>(type: "integer", nullable: false),
                    QuietDaysUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactEmailCampaignLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactEmailCampaignLogs_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactEmailCampaignLogs_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmailCampaignLogs_BusinessId_ContactId_SentAt",
                table: "ContactEmailCampaignLogs",
                columns: new[] { "BusinessId", "ContactId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmailCampaignLogs_BusinessId_SentAt",
                table: "ContactEmailCampaignLogs",
                columns: new[] { "BusinessId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmailCampaignLogs_ContactId",
                table: "ContactEmailCampaignLogs",
                columns: new[] { "ContactId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactEmailCampaignLogs");

            migrationBuilder.DropColumn(
                name: "LastMarketingEmailAt",
                table: "Contacts");
        }
    }
}
