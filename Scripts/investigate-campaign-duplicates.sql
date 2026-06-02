-- Diagnóstico: campañas duplicadas / reenvíos (Supabase SQL Editor)

-- 1) Campañas recientes
SELECT "Id", "Status", "TotalRecipients", "SentCount", "FailedCount", "CreatedAt", "CompletedAt", "SubjectTemplate"
FROM "EmailCampaigns"
ORDER BY "CreatedAt" DESC
LIMIT 10;

-- 2) Destinatarios por estado (global)
SELECT "Status", COUNT(*) AS total
FROM "EmailCampaignRecipients"
GROUP BY "Status"
ORDER BY total DESC;

-- 3) ¿Mismo correo marcado Pending muchas veces? (reenvíos)
SELECT "ContactEmail", "Status", COUNT(*) AS veces, MIN("GlobalQueueOrder") AS primero, MAX("ProcessedAt") AS ultimo
FROM "EmailCampaignRecipients"
GROUP BY "ContactEmail", "Status"
ORDER BY veces DESC;

-- 4) Logs de envío hoy (si la tabla existe)
SELECT COUNT(*) AS logs_hoy
FROM "ContactEmailCampaignLogs"
WHERE "Status" = 'Sent' AND "SentAt" >= CURRENT_DATE;

-- 5) Comparar: envíos reales (Sent en recipients) vs pendientes
SELECT
  (SELECT COUNT(*) FROM "EmailCampaignRecipients" WHERE "Status" = 'Sent') AS recipients_sent,
  (SELECT COUNT(*) FROM "EmailCampaignRecipients" WHERE "Status" = 'Pending') AS recipients_pending,
  (SELECT COUNT(*) FROM "EmailCampaignRecipients" WHERE "Status" = 'Processing') AS recipients_processing;

-- Si recipients_pending = 4 pero recibiste decenas de correos al mismo email,
-- el worker reenvió porque no pudo marcar Sent (falló SaveChanges, ej. tabla ContactEmailCampaignLogs faltante).
-- Solución: cancel-active-campaigns.sql + aplicar sección 6 de apply-pending-migrations.sql + deploy fix API.
