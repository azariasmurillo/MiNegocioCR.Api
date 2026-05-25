-- Seed: Business, BusinessSettings, RepairOrders y WhatsAppConversations
-- Ejecutar con el business_id real obtenido en login/sesion.
-- Ejemplo:
--   psql ... -v business_id=EL_GUID_DEL_LOGIN -f Migrations/SeedWhatsAppConversations.sql
-- Teléfonos: 50662946202, 50661347303

-- 1. Business (si no existe). Necesario porque WhatsAppConversations tiene FK a Businesses.
INSERT INTO "Businesses" (
    "Id",
    "Name",
    "CreatedAt",
    "IsActive",
    "EnableEmailNotifications",
    "EnableWhatsappNotifications"
)
VALUES (
    :'business_id'::uuid,
    'Mi Negocio CR',
    NOW(),
    true,
    true,
    true
)
ON CONFLICT ("Id") DO NOTHING;

-- 2. BusinessSettings (requerido para el negocio; 1:1 con Business)
INSERT INTO "BusinessSettings" (
    "BusinessId",
    "EnableAIChat",
    "NextOrderNumber"
)
VALUES (
    :'business_id'::uuid,
    false,
    1
)
ON CONFLICT ("BusinessId") DO NOTHING;

-- 3. RepairOrders (para poder vincular una conversación a una orden)
-- Status: 1=Pending, 2=InProcess, 3=Processed, 4=Delivered, 5=Cancelled
INSERT INTO "RepairOrders" (
    "Id",
    "BusinessId",
    "OrderNumber",
    "CustomerName",
    "CustomerPhone",
    "Status",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
    (
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::uuid,
        :'business_id'::uuid,
        1,
        'María Rojas',
        '50662946202',
        1, -- Pending
        NOW() - INTERVAL '1 day',
        NOW()
    ),
    (
        'b1ffcc00-0d1c-4f09-cc4f-7c81b9502c33'::uuid,
        :'business_id'::uuid,
        2,
        'Juan Pérez',
        '50661347303',
        2, -- InProcess
        NOW() - INTERVAL '2 hours',
        NOW()
    )
ON CONFLICT ("Id") DO NOTHING;

-- 4. WhatsAppConversations (Status: 0=Open, 1=Pending, 2=Closed)
INSERT INTO "WhatsAppConversations" (
    "Id",
    "BusinessId",
    "PhoneNumber",
    "CustomerName",
    "LastMessage",
    "LastMessageAt",
    "UnreadCount",
    "CreatedAt",
    "IsArchived",
    "RepairOrderId",
    "Status"
)
VALUES
    (
        'c2a1dd11-1e2d-5d4f-9e5a-8c91c061d444'::uuid,
        :'business_id'::uuid,
        '50662946202',
        'María Rojas',
        'Hola! ¿Cómo estás?',
        NOW() - INTERVAL '30 minutes',
        2,
        NOW() - INTERVAL '1 day',
        false,
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::uuid, -- vinculada a la orden de María
        0 -- Open
    ),
    (
        'd3b2ee22-2f3e-6e5a-0f6b-9d02d172e555'::uuid,
        :'business_id'::uuid,
        '50661347303',
        'Juan Pérez',
        'Perfecto, te confirmo en un momento.',
        NOW() - INTERVAL '5 minutes',
        0,
        NOW() - INTERVAL '2 hours',
        false,
        'b1ffcc00-0d1c-4f09-cc4f-7c81b9502c33'::uuid, -- vinculada a la orden de Juan
        0 -- Open
    )
ON CONFLICT ("BusinessId", "PhoneNumber") DO UPDATE SET
    "CustomerName"     = EXCLUDED."CustomerName",
    "LastMessage"      = EXCLUDED."LastMessage",
    "LastMessageAt"   = EXCLUDED."LastMessageAt",
    "UnreadCount"     = EXCLUDED."UnreadCount",
    "IsArchived"      = EXCLUDED."IsArchived",
    "RepairOrderId"   = EXCLUDED."RepairOrderId",
    "Status"           = EXCLUDED."Status";
