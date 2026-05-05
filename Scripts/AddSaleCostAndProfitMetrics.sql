-- Métricas de costo/ganancia por venta (PostgreSQL / Supabase).
-- Tablas alineadas con EF Core: "SaleItems", "Sales".
-- Filas existentes quedan en 0 (sin backfill automático).
--
-- Si la API ya usa `Database.Migrate()` al arrancar, normalmente NO hace falta ejecutar este archivo:
-- basta con reiniciar la API para que EF aplique la migración.
--
-- Si ejecutás este script a mano y después usás Migrate(), registrá la migración para evitar error
-- "column already exists" (descomentá la última sección si hace falta).

ALTER TABLE "SaleItems"
    ADD COLUMN IF NOT EXISTS "CostPrice" NUMERIC(18, 2) NOT NULL DEFAULT 0;

ALTER TABLE "Sales"
    ADD COLUMN IF NOT EXISTS "TotalCost" NUMERIC(18, 2) NOT NULL DEFAULT 0;

ALTER TABLE "Sales"
    ADD COLUMN IF NOT EXISTS "TotalProfit" NUMERIC(18, 2) NOT NULL DEFAULT 0;

-- Constraints básicos (costos no negativos). TotalProfit puede ser negativo en edge cases.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'CK_SaleItems_CostPrice_NonNegative') THEN
        ALTER TABLE "SaleItems"
            ADD CONSTRAINT "CK_SaleItems_CostPrice_NonNegative" CHECK ("CostPrice" >= 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'CK_Sales_TotalCost_NonNegative') THEN
        ALTER TABLE "Sales"
            ADD CONSTRAINT "CK_Sales_TotalCost_NonNegative" CHECK ("TotalCost" >= 0);
    END IF;
END $$;

-- Opcional: solo si aplicaste este SQL manualmente y la API también ejecuta Migrate al inicio.
-- Descomentá si Migrate falla diciendo que la columna ya existe:
-- INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
-- VALUES ('20260504220000_AddSaleCostAndProfitMetrics', '8.0.8')
-- ON CONFLICT ("MigrationId") DO NOTHING;
