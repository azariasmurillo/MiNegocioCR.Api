-- EMERGENCIA: detener todas las campañas de correo en curso (Supabase SQL Editor)
-- Ejecutá esto si los correos no paran. Es idempotente.

BEGIN;

UPDATE "EmailCampaigns"
SET "Status" = 'Cancelled',
    "CompletedAt" = NOW()
WHERE "Status" IN ('Queued', 'InProgress');

UPDATE "EmailCampaignRecipients"
SET "Status" = 'Cancelled',
    "ProcessedAt" = NOW(),
    "ErrorMessage" = 'Campaña cancelada manualmente (emergencia).'
WHERE "Status" IN ('Pending', 'Processing');

COMMIT;

-- Verificar que no queden pendientes:
-- SELECT COUNT(*) FROM "EmailCampaignRecipients" WHERE "Status" = 'Pending';
-- SELECT "Id", "Status", "TotalRecipients", "SentCount" FROM "EmailCampaigns" ORDER BY "CreatedAt" DESC LIMIT 5;
