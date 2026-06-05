-- =============================================================================
-- MiNegocioCR — Créditos / cuentas por cobrar (manual, idempotente)
-- =============================================================================
-- Migración EF: 20260605163023_AddCreditAccounts
--
-- CUÁNDO USAR:
--   • Deploy a Supabase/prod sin `dotnet ef database update`
--   • Respaldo si EF falla pero necesitás las tablas Credit*
--
-- LOCAL:
--   psql -U postgres -d MiNegocioCR_Dev -f Scripts/apply-credit-accounts-migration.sql
--
-- PRODUCCIÓN (conexión DIRECTA puerto 5432, NO pooler 6543):
--   psql "<POSTGRES_CONNECTION_STRING>" -f Scripts/apply-credit-accounts-migration.sql
--
-- VERIFICAR DESPUÉS:
--   SELECT "MigrationId" FROM "__EFMigrationsHistory"
--   WHERE "MigrationId" = '20260605163023_AddCreditAccounts';
--
--   SELECT table_name FROM information_schema.tables
--   WHERE table_schema = 'public' AND table_name LIKE 'Credit%'
--   ORDER BY 1;
-- =============================================================================

BEGIN;

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
