-- Verificación rápida del schema requerido por dashboard y ventas (mayo 2026).
-- Ejecutar: psql "<connection_string>" -f Scripts/verify-schema.sql

\echo '=== Columnas Sales (financiero + descuentos) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Sales'
  AND column_name IN (
    'TotalProfit', 'TotalCost', 'TotalOrden', 'PrepaidAmount',
    'DiscountAmount', 'DiscountKind', 'DiscountInputValue'
  )
ORDER BY column_name;

\echo '=== Tabla SalePaymentMethods ==='
SELECT EXISTS (
  SELECT 1 FROM information_schema.tables
  WHERE table_schema = 'public' AND table_name = 'SalePaymentMethods'
) AS sale_payment_methods_exists;

\echo '=== Migraciones recientes en historial ==='
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
WHERE "MigrationId" IN (
  '20260504220000_AddSaleCostAndProfitMetrics',
  '20260522120000_RefactorPaymentsAndSalePaymentMethods',
  '20260526120000_RemoveRepairOrderDiscountPercent',
  '20260526130000_AddSaleDiscountMetadata'
)
ORDER BY "MigrationId";

\echo '=== Esperado ==='
-- Sales: 7 columnas listadas arriba
-- SalePaymentMethods: true
-- Historial: 4 filas (o las que falten indican qué migración correr)
