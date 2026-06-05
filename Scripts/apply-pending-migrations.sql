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
--   • 20260604142659_AddInternetOrders
--   • 20260605163023_AddCreditAccounts
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

-- ── 8. Pedidos Internet (Amazon / proxy) ─────────────────────────────────────
CREATE TABLE IF NOT EXISTS "InternetOrders" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "OrderNumber" character varying(20) NOT NULL,
  "Status" integer NOT NULL,
  "ExchangeRateApplied" numeric(18,4) NOT NULL,
  "InternationalShippingCost" numeric(18,2) NOT NULL,
  "LocalCourierCost" numeric(18,2) NOT NULL,
  "ServiceFee" numeric(18,2) NOT NULL,
  "LinesTotalUsd" numeric(18,2) NOT NULL,
  "LinesTotalCrc" numeric(18,2) NOT NULL,
  "SubtotalCrc" numeric(18,2) NOT NULL,
  "TotalAdvancesCrc" numeric(18,2) NOT NULL,
  "BalanceDueCrc" numeric(18,2) NOT NULL,
  "CustomerNotes" character varying(2000),
  "InternalNotes" character varying(2000),
  "RefundNote" character varying(2000),
  "ExternalOrderId" character varying(100),
  "TrackingNumber" character varying(200),
  "CreatedAt" timestamp with time zone NOT NULL,
  "UpdatedAt" timestamp with time zone NOT NULL,
  "PurchasedAt" timestamp with time zone,
  "ReceivedAt" timestamp with time zone,
  "DeliveredAt" timestamp with time zone,
  "CancelledAt" timestamp with time zone,
  CONSTRAINT "PK_InternetOrders" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_InternetOrders_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_InternetOrders_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "InternetOrderLines" (
  "Id" uuid NOT NULL,
  "InternetOrderId" uuid NOT NULL,
  "SortOrder" integer NOT NULL,
  "ProductName" character varying(300) NOT NULL,
  "ProductUrl" character varying(2000) NOT NULL,
  "UnitPriceUsd" numeric(18,2) NOT NULL,
  "Quantity" integer NOT NULL,
  "LineTotalUsd" numeric(18,2) NOT NULL,
  "LineTotalCrc" numeric(18,2) NOT NULL,
  CONSTRAINT "PK_InternetOrderLines" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_InternetOrderLines_InternetOrders_InternetOrderId" FOREIGN KEY ("InternetOrderId") REFERENCES "InternetOrders" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "InternetOrderAdvances" (
  "Id" uuid NOT NULL,
  "InternetOrderId" uuid NOT NULL,
  "AmountCrc" numeric(18,2) NOT NULL,
  "PaidAt" timestamp with time zone NOT NULL,
  "Method" character varying(50),
  "Notes" character varying(500),
  CONSTRAINT "PK_InternetOrderAdvances" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_InternetOrderAdvances_InternetOrders_InternetOrderId" FOREIGN KEY ("InternetOrderId") REFERENCES "InternetOrders" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_InternetOrders_BusinessId_OrderNumber" ON "InternetOrders" ("BusinessId", "OrderNumber");
CREATE INDEX IF NOT EXISTS "IX_InternetOrders_BusinessId_Status_CreatedAt" ON "InternetOrders" ("BusinessId", "Status", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_InternetOrders_ContactId" ON "InternetOrders" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_InternetOrderLines_InternetOrderId" ON "InternetOrderLines" ("InternetOrderId");
CREATE INDEX IF NOT EXISTS "IX_InternetOrderAdvances_InternetOrderId" ON "InternetOrderAdvances" ("InternetOrderId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260604142659_AddInternetOrders', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604142659_AddInternetOrders'
);

-- ── 9. Créditos / cuentas por cobrar ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "CreditAccounts" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "AccountNumber" character varying(20) NOT NULL,
  "Status" integer NOT NULL,
  "CurrentBalanceCrc" numeric(18,2) NOT NULL,
  "TotalChargedCrc" numeric(18,2) NOT NULL,
  "PaymentCommitmentDate" timestamp with time zone,
  "Notes" character varying(2000),
  "CreatedAt" timestamp with time zone NOT NULL,
  "UpdatedAt" timestamp with time zone NOT NULL,
  "PaidAt" timestamp with time zone,
  "CancelledAt" timestamp with time zone,
  CONSTRAINT "PK_CreditAccounts" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_CreditAccounts_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_CreditAccounts_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "CreditCommunications" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "CreditAccountId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "CommunicationType" integer NOT NULL,
  "Notes" character varying(2000),
  "CreatedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "PK_CreditCommunications" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_CreditCommunications_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_CreditCommunications_CreditAccounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "CreditAccounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "CreditTransactions" (
  "Id" uuid NOT NULL,
  "BusinessId" uuid NOT NULL,
  "CreditAccountId" uuid NOT NULL,
  "ContactId" uuid NOT NULL,
  "TransactionType" integer NOT NULL,
  "AmountCrc" numeric(18,2) NOT NULL,
  "AppliedToBalanceCrc" numeric(18,2),
  "ChangeGivenCrc" numeric(18,2),
  "Description" character varying(500),
  "PreviousBalanceCrc" numeric(18,2) NOT NULL,
  "NewBalanceCrc" numeric(18,2) NOT NULL,
  "Notes" character varying(2000),
  "CreatedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "PK_CreditTransactions" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_CreditTransactions_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_CreditTransactions_CreditAccounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "CreditAccounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "CreditTransactionLines" (
  "Id" uuid NOT NULL,
  "CreditTransactionId" uuid NOT NULL,
  "SortOrder" integer NOT NULL,
  "LineKind" integer NOT NULL,
  "CatalogVariantId" uuid,
  "ConceptName" character varying(300) NOT NULL,
  "Quantity" integer NOT NULL,
  "BaseUnitPriceCrc" numeric(18,2) NOT NULL,
  "CreditMarkupPercent" numeric(5,2) NOT NULL,
  "UnitPriceCrc" numeric(18,2) NOT NULL,
  "LineTotalCrc" numeric(18,2) NOT NULL,
  CONSTRAINT "PK_CreditTransactionLines" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_CreditTransactionLines_CatalogVariants_CatalogVariantId" FOREIGN KEY ("CatalogVariantId") REFERENCES "CatalogVariants" ("Id") ON DELETE RESTRICT,
  CONSTRAINT "FK_CreditTransactionLines_CreditTransactions_CreditTransactionId" FOREIGN KEY ("CreditTransactionId") REFERENCES "CreditTransactions" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_CreditAccounts_BusinessId_AccountNumber" ON "CreditAccounts" ("BusinessId", "AccountNumber");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_CreditAccounts_BusinessId_ContactId" ON "CreditAccounts" ("BusinessId", "ContactId");
CREATE INDEX IF NOT EXISTS "IX_CreditAccounts_BusinessId_Status_CurrentBalanceCrc" ON "CreditAccounts" ("BusinessId", "Status", "CurrentBalanceCrc");
CREATE INDEX IF NOT EXISTS "IX_CreditAccounts_ContactId" ON "CreditAccounts" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_CreditCommunications_ContactId" ON "CreditCommunications" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_CreditCommunications_CreditAccountId" ON "CreditCommunications" ("CreditAccountId");
CREATE INDEX IF NOT EXISTS "IX_CreditTransactionLines_CatalogVariantId" ON "CreditTransactionLines" ("CatalogVariantId");
CREATE INDEX IF NOT EXISTS "IX_CreditTransactionLines_CreditTransactionId" ON "CreditTransactionLines" ("CreditTransactionId");
CREATE INDEX IF NOT EXISTS "IX_CreditTransactions_BusinessId_CreatedAt" ON "CreditTransactions" ("BusinessId", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_CreditTransactions_ContactId" ON "CreditTransactions" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_CreditTransactions_CreditAccountId_CreatedAt" ON "CreditTransactions" ("CreditAccountId", "CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260605163023_AddCreditAccounts', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605163023_AddCreditAccounts'
);


COMMIT;
