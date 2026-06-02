-- Verificación rápida del schema requerido por dashboard y ventas (mayo 2026).
-- Ejecutar: psql "<connection_string>" -f Scripts/verify-schema.sql

\echo '=== Columnas Sales (financiero + descuentos) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Sales'
  AND column_name IN (
    'TotalProfit', 'TotalCost', 'TotalOrden', 'PrepaidAmount',
    'Discount', 'DiscountKind', 'DiscountInputValue', 'TotalAmount'
  )
ORDER BY column_name;

\echo '=== Tabla SalePaymentMethods ==='
SELECT EXISTS (
  SELECT 1 FROM information_schema.tables
  WHERE table_schema = 'public' AND table_name = 'SalePaymentMethods'
) AS sale_payment_methods_exists;

\echo '=== Columnas Contacts (CRM) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Contacts'
  AND column_name IN ('LastActivityAt', 'LastMarketingEmailAt');

\echo '=== Tabla ContactEmailCampaignLogs ==='
SELECT EXISTS (
  SELECT 1 FROM information_schema.tables
  WHERE table_schema = 'public' AND table_name = 'ContactEmailCampaignLogs'
) AS contact_email_campaign_logs_exists;

\echo '=== Tablas cola de campañas ==='
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('EmailCampaigns', 'EmailCampaignRecipients')
ORDER BY table_name;

\echo '=== Migraciones recientes en historial ==='
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
WHERE "MigrationId" IN (
  '20260504220000_AddSaleCostAndProfitMetrics',
  '20260522120000_RefactorPaymentsAndSalePaymentMethods',
  '20260526120000_RemoveRepairOrderDiscountPercent',
  '20260526130000_AddSaleDiscountMetadata',
  '20260527120000_AddContactLastActivityAt',
  '20260528120000_AddContactEmailCampaign',
  '20260529120000_AddEmailCampaignQueue'
)
ORDER BY "MigrationId";

\echo '=== Esperado ==='
-- Sales: 8 columnas listadas arriba (Discount = monto descuento; TotalAmount legacy = Total)
-- SalePaymentMethods: true
-- Contacts: LastActivityAt + LastMarketingEmailAt (2 filas en consulta CRM)
-- ContactEmailCampaignLogs + EmailCampaigns + EmailCampaignRecipients: existen
-- Historial: 7 filas mayo 2026 (o las que falten indican qué migración correr)
