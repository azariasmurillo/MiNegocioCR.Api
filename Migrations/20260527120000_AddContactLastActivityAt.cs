using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContactLastActivityAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Contacts" c
                SET "LastActivityAt" = sub.max_at
                FROM (
                    SELECT contact_id, MAX(at) AS max_at
                    FROM (
                        SELECT ro."ContactId" AS contact_id, p."CreatedAt" AS at
                        FROM "Payments" p
                        INNER JOIN "RepairOrders" ro ON ro."Id" = p."RepairOrderId"
                        WHERE p."Amount" > 0

                        UNION ALL

                        SELECT s."ContactId" AS contact_id, s."SaleDate" AS at
                        FROM "Sales" s
                        WHERE s."ContactId" IS NOT NULL
                          AND (
                            s."Total" > 0
                            OR s."PrepaidAmount" > 0
                            OR EXISTS (
                                SELECT 1
                                FROM "SalePaymentMethods" spm
                                WHERE spm."SaleId" = s."Id" AND spm."Amount" > 0
                            )
                          )
                    ) events
                    WHERE contact_id IS NOT NULL
                    GROUP BY contact_id
                ) sub
                WHERE c."Id" = sub.contact_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "Contacts");
        }
    }
}
