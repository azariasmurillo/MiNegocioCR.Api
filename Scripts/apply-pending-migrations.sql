-- Migraciones huérfanas (sin Designer) que EF no aplica con dotnet ef database update.
-- Ejecutar una vez en PostgreSQL local después de la primera migración:
--   psql -U postgres -d MiNegocioCR_Dev -f scripts/apply-pending-migrations.sql

BEGIN;

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
SELECT '20260504220000_AddSaleCostAndProfitMetrics', '8.0.4'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260504220000_AddSaleCostAndProfitMetrics'
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260522120000_RefactorPaymentsAndSalePaymentMethods', '8.0.4'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260522120000_RefactorPaymentsAndSalePaymentMethods'
);

COMMIT;
