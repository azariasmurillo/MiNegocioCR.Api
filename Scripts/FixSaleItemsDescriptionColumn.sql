-- Idempotent fix for production environments missing SaleItems.Description
ALTER TABLE "SaleItems"
ADD COLUMN IF NOT EXISTS "Description" text NULL;
