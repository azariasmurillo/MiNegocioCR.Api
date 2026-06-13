using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantImageImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImportBatchId",
                table: "CatalogVariantImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileImageUrl",
                table: "CatalogVariantImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "CatalogVariantImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceFileName",
                table: "CatalogVariantImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailImageUrl",
                table: "CatalogVariantImages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StagingZipPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReplaceExisting = table.Column<bool>(type: "boolean", nullable: false),
                    UseBackgroundRemoval = table.Column<bool>(type: "boolean", nullable: false),
                    MarketplaceStyle = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalFiles = table.Column<int>(type: "integer", nullable: false),
                    ProcessedFiles = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    SummaryMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageImportBatches_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageImportLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ParsedSku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    CatalogVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageImportLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageImportLogs_ImageImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ImageImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogVariantImages_CatalogVariantId_SortOrder",
                table: "CatalogVariantImages",
                columns: new[] { "CatalogVariantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageImportBatches_BusinessId",
                table: "ImageImportBatches",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageImportBatches_Status_CreatedAt",
                table: "ImageImportBatches",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageImportLogs_BatchId",
                table: "ImageImportLogs",
                column: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageImportLogs");

            migrationBuilder.DropTable(
                name: "ImageImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_CatalogVariantImages_CatalogVariantId_SortOrder",
                table: "CatalogVariantImages");

            migrationBuilder.DropColumn(
                name: "ImportBatchId",
                table: "CatalogVariantImages");

            migrationBuilder.DropColumn(
                name: "MobileImageUrl",
                table: "CatalogVariantImages");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "CatalogVariantImages");

            migrationBuilder.DropColumn(
                name: "SourceFileName",
                table: "CatalogVariantImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailImageUrl",
                table: "CatalogVariantImages");
        }
    }
}
