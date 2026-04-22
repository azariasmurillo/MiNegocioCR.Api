-- Script de respaldo / referencia: backfill de RepairOrders.ContactId desde columnas legacy.
-- La migración EF 20260422223000_RepairOrderContactId ejecuta la misma lógica en Up().
--
-- Reglas:
-- - Teléfono = TRIM(REPLACE(COALESCE("CustomerPhone", ''), '+', '')); si queda vacío → 'LEGACY-' || replace("Id"::text, '-', '')
-- - Buscar Contact por ("BusinessId", Phone); si no existe, INSERT en Contacts
-- - UPDATE RepairOrders SET "ContactId" = ...
--
-- NO ejecutar dos veces sobre una BD ya migrada (columnas legacy ya no existen).

DO $$
DECLARE
  r RECORD;
  v_phone text;
  v_contact_id uuid;
BEGIN
  FOR r IN
    SELECT ro."Id", ro."BusinessId", ro."CustomerName", ro."CustomerPhone", ro."CustomerEmail"
    FROM "RepairOrders" ro
    WHERE ro."ContactId" IS NULL
  LOOP
    v_phone := NULLIF(TRIM(REPLACE(COALESCE(r."CustomerPhone", ''), '+', '')), '');
    IF v_phone IS NULL OR v_phone = '' THEN
      v_phone := 'LEGACY-' || REPLACE(r."Id"::text, '-', '');
    END IF;

    SELECT c."Id" INTO v_contact_id
    FROM "Contacts" c
    WHERE c."BusinessId" = r."BusinessId" AND c."Phone" = v_phone
    LIMIT 1;

    IF v_contact_id IS NULL THEN
      v_contact_id := gen_random_uuid();
      INSERT INTO "Contacts" ("Id", "BusinessId", "Name", "Phone", "Email", "CreatedAt")
      VALUES (
        v_contact_id,
        r."BusinessId",
        COALESCE(NULLIF(TRIM(r."CustomerName"), ''), 'Sin nombre'),
        v_phone,
        NULLIF(TRIM(r."CustomerEmail"), ''),
        NOW()
      );
    END IF;

    UPDATE "RepairOrders" SET "ContactId" = v_contact_id WHERE "Id" = r."Id";
  END LOOP;
END $$;
