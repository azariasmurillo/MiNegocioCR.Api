using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrderContactId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "RepairOrders",
                type: "uuid",
                nullable: true);

            // Backfill: por orden, contacto existente por (BusinessId, Phone) o creación; sin teléfono → Phone único LEGACY-{Id}.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                  r RECORD;
                  v_phone text;
                  v_contact_id uuid;
                BEGIN
                  FOR r IN
                    SELECT ro."Id", ro."BusinessId", ro."CustomerName", ro."CustomerPhone", ro."CustomerEmail"
                    FROM "RepairOrders" ro
                    WHERE ro."ContactId" IS NULL
                  LOOP
                    v_phone := NULLIF(TRIM(REPLACE(COALESCE(r."CustomerPhone", ''), '+', '')), '');
                    IF v_phone IS NULL OR v_phone = '' THEN
                      v_phone := 'LEGACY-' || REPLACE(r."Id"::text, '-', '');
                    END IF;

                    SELECT c."Id" INTO v_contact_id
                    FROM "Contacts" c
                    WHERE c."BusinessId" = r."BusinessId" AND c."Phone" = v_phone
                    LIMIT 1;

                    IF v_contact_id IS NULL THEN
                      v_contact_id := gen_random_uuid();
                      INSERT INTO "Contacts" ("Id", "BusinessId", "Name", "Phone", "Email", "CreatedAt")
                      VALUES (
                        v_contact_id,
                        r."BusinessId",
                        COALESCE(NULLIF(TRIM(r."CustomerName"), ''), 'Sin nombre'),
                        v_phone,
                        NULLIF(TRIM(r."CustomerEmail"), ''),
                        NOW()
                      );
                    END IF;

                    UPDATE "RepairOrders" SET "ContactId" = v_contact_id WHERE "Id" = r."Id";
                  END LOOP;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ContactId",
                table: "RepairOrders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_ContactId",
                table: "RepairOrders",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Contacts_ContactId",
                table: "RepairOrders",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "RepairOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Contacts_ContactId",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_ContactId",
                table: "RepairOrders");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "RepairOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "RepairOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "RepairOrders",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "RepairOrders" r
                SET "CustomerName" = c."Name",
                    "CustomerPhone" = c."Phone",
                    "CustomerEmail" = c."Email"
                FROM "Contacts" c
                WHERE r."ContactId" = c."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "RepairOrders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "RepairOrders");
        }
    }
}

