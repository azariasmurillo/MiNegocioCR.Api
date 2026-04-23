using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrderTechnicalDeviceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessoriesIncluded",
                table: "RepairOrders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "RepairOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "RepairOrders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceTypeOther",
                table: "RepairOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "RepairOrders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "RepairOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "RepairOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "RepairOrders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessoriesIncluded",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "DeviceTypeOther",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "RepairOrders");
        }
    }
}
