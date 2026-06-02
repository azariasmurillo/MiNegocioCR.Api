-- =============================================================================
-- MiNegocioCR — Schema manual (idempotente) — Mayo 2026
-- =============================================================================
-- Usar cuando `dotnet ef database update` no corra o falle en local/producción.
-- El script es seguro de ejecutar varias veces (IF NOT EXISTS / INSERT condicional).
--
-- ORDEN RECOMENDADO:
--   1. dotnet ef database update          ← preferido (desde MiNegocioCR.Api/)
--   2. Este archivo                       ← respaldo manual
--   3. Scripts/verify-schema.sql          ← comprobar columnas e historial
--
-- LOCAL (PowerShell, desde la raíz del repo):
--   psql -U postgres -d MiNegocioCR_Dev -f MiNegocioCR.Api/Scripts/apply-pending-migrations.sql
--
-- PRODUCCIÓN Supabase (conexión DIRECTA puerto 5432, NO pooler 6543):
--   psql "<POSTGRES_CONNECTION_STRING>" -f MiNegocioCR.Api/Scripts/apply-pending-migrations.sql
--
-- MIGRACIONES QUE CUBRE (registra en __EFMigrationsHistory):
--   • 20260504220000_AddSaleCostAndProfitMetrics
--   • 20260522120000_RefactorPaymentsAndSalePaymentMethods
--   • 20260526120000_RemoveRepairOrderDiscountPercent
--   • 20260526130000_AddSaleDiscountMetadata
--   • 20260527120000_AddContactLastActivityAt
--   • 20260528120000_AddContactEmailCampaign
--   • 20260529120000_AddEmailCampaignQueue
--
-- NOTA: DiscountAmount en Sales ya existía antes de mayo 2026; no se agrega aquí.
-- =============================================================================

BEGIN;

-- ── 1. Costos y ganancia en ventas ───────────────────────────────────────────
ALTER TABLE "SaleItems" ADD COLUMN IF NOT EXISTS "CostPrice" numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "TotalCost" numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "TotalProfit" numeric(18,2) NOT NULL DEFAULT 0;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_SaleItems_CostPrice_NonNegative') THEN
    ALTER TABLE "SaleItems" ADD CONSTRAINT "CK_SaleItems_CostPrice_NonNegative" CHECK ("CostPrice" >= 0);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_Sales_TotalCost_NonNegative') THEN
    ALTER TABLE "Sales" ADD CONSTRAINT "CK_Sales_TotalCost_NonNegative" CHECK ("TotalCost" >= 0);
  END IF;
END $$;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260504220000_AddSaleCostAndProfitMetrics', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260504220000_AddSaleCostAndProfitMetrics'
);

-- ── 2. Pagos mixtos en ventas (SalePaymentMethods) ─────────────────────────
CREATE TABLE IF NOT EXISTS "SalePaymentMethods" (
  "Id" uuid NOT NULL,
  "SaleId" uuid NOT NULL,
  "Method" integer NOT NULL,
  "Amount" numeric(18,2) NOT NULL DEFAULT 0,
  CONSTRAINT "PK_SalePaymentMethods" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_SalePaymentMethods_Sales_SaleId" FOREIGN KEY ("SaleId") REFERENCES "Sales" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_SalePaymentMethods_SaleId" ON "SalePaymentMethods" ("SaleId");

ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "TotalOrden" numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "PrepaidAmount" numeric(18,2) NOT NULL DEFAULT 0;

UPDATE "Sales"
SET "TotalOrden" = COALESCE(NULLIF("Total", 0), "TotalAmount", 0),
    "PrepaidAmount" = 0
WHERE "TotalOrden" = 0;

ALTER TABLE "Sales" DROP COLUMN IF EXISTS "PayCash";
ALTER TABLE "Sales" DROP COLUMN IF EXISTS "PayTransfer";
ALTER TABLE "Sales" DROP COLUMN IF EXISTS "PaySinpe";
ALTER TABLE "Sales" DROP COLUMN IF EXISTS "PayCard";

ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "PayCash";
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "PayTransfer";
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "PaySinpe";
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "PayCard";

ALTER TABLE "Payments" ADD COLUMN IF NOT EXISTS "Reference" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260522120000_RefactorPaymentsAndSalePaymentMethods', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260522120000_RefactorPaymentsAndSalePaymentMethods'
);

-- ── 3. Quitar descuento legacy de órdenes de reparación ─────────────────────
-- El descuento vive en la venta (Sales), no en RepairOrders.
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "DiscountPercent";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260526120000_RemoveRepairOrderDiscountPercent', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260526120000_RemoveRepairOrderDiscountPercent'
);

-- ── 4. Metadata de descuento en ventas ───────────────────────────────────────
-- DiscountKind: 0=None, 1=Percent, 2=FixedAmount (SaleDiscountKind)
-- DiscountInputValue: valor ingresado (% o ₡ según DiscountKind)
-- DiscountAmount: monto aplicado en colones (columna preexistente en Sales)
ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "DiscountKind" smallint NOT NULL DEFAULT 0;
ALTER TABLE "Sales" ADD COLUMN IF NOT EXISTS "DiscountInputValue" numeric(18,2) NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260526130000_AddSaleDiscountMetadata', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260526130000_AddSaleDiscountMetadata'
);

-- ── 5. Última actividad comercial en contactos (CRM Fase 1) ─────────────────
ALTER TABLE "Contacts" ADD COLUMN IF NOT EXISTS "LastActivityAt" timestamp with time zone;

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
WHERE c."Id" = sub.contact_id
  AND (c."LastActivityAt" IS NULL OR c."LastActivityAt" < sub.max_at);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260527120000_AddContactLastActivityAt', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260527120000_AddContactLastActivityAt'
);

-- ── 6. Campañas de correo (CRM Fase 2) ───────────────────────────────────────
ALTER TABLE "Contacts" ADD COLUMN IF NOT EXISTS "LastMarketingEmailAt" timestamp with time zone;

CREATE TABLE IF NOT EXISTS "ContactEmailCampaignLogs" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "SentAt" timestamp with time zone NOT NULL,
  "Subject" character varying(300) NOT NULL,
  "Status" character varying(20) NOT NULL,
  "ResendMessageId" character varying(100),
  "ErrorMessage" character varying(500),
  "InactiveDaysUsed" integer NOT NULL,
  "QuietDaysUsed" integer NOT NULL,
  CONSTRAINT "PK_ContactEmailCampaignLogs" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_ContactEmailCampaignLogs_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_ContactEmailCampaignLogs_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_ContactEmailCampaignLogs_BusinessId_SentAt" ON "ContactEmailCampaignLogs" ("BusinessId", "SentAt");
CREATE INDEX IF NOT EXISTS "IX_ContactEmailCampaignLogs_BusinessId_ContactId_SentAt" ON "ContactEmailCampaignLogs" ("BusinessId", "ContactId", "SentAt");
CREATE INDEX IF NOT EXISTS "IX_ContactEmailCampaignLogs_ContactId" ON "ContactEmailCampaignLogs" ("ContactId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260528120000_AddContactEmailCampaign', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120000_AddContactEmailCampaign'
);

-- ── 7. Cola de campañas por correo ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "EmailCampaigns" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "SubjectTemplate" character varying(300) NOT NULL,
  "BodyText" character varying(8000),
  "ImageUrl" character varying(2000),
  "InactiveDaysUsed" integer NOT NULL,
  "QuietDaysUsed" integer NOT NULL,
  "AudienceMode" character varying(32) NOT NULL,
  "Status" character varying(20) NOT NULL,
  "CreatedAt" timestamp with time zone NOT NULL,
  "CompletedAt" timestamp with time zone,
  "TotalRecipients" integer NOT NULL,
  "SentCount" integer NOT NULL,
  "FailedCount" integer NOT NULL,
  CONSTRAINT "PK_EmailCampaigns" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_EmailCampaigns_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "EmailCampaignRecipients" (
  "Id" uuid NOT NULL,
  "CampaignId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "ContactName" character varying(200) NOT NULL,
  "ContactEmail" character varying(200) NOT NULL,
  "Status" character varying(20) NOT NULL,
  "GlobalQueueOrder" bigint NOT NULL,
  "ProcessedAt" timestamp with time zone,
  "ErrorMessage" character varying(500),
  "ResendMessageId" character varying(100),
  CONSTRAINT "PK_EmailCampaignRecipients" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_EmailCampaignRecipients_EmailCampaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES "EmailCampaigns" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_EmailCampaignRecipients_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_EmailCampaigns_BusinessId_CreatedAt" ON "EmailCampaigns" ("BusinessId", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_EmailCampaigns_BusinessId_Status" ON "EmailCampaigns" ("BusinessId", "Status");
CREATE INDEX IF NOT EXISTS "IX_EmailCampaignRecipients_CampaignId" ON "EmailCampaignRecipients" ("CampaignId");
CREATE INDEX IF NOT EXISTS "IX_EmailCampaignRecipients_ContactId" ON "EmailCampaignRecipients" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_EmailCampaignRecipients_Status_GlobalQueueOrder" ON "EmailCampaignRecipients" ("Status", "GlobalQueueOrder");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260529120000_AddEmailCampaignQueue', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260529120000_AddEmailCampaignQueue'
);


COMMIT;
