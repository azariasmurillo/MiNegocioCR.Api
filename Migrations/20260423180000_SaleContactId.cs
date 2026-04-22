using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class SaleContactId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ContactId",
                table: "Sales",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Contacts_ContactId",
                table: "Sales",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Contacts_ContactId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_ContactId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Sales");
        }
    }
}
