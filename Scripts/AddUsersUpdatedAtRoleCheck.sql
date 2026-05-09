-- Incremental: alinea "Users" con Id, Email único, PasswordHash, Role (Admin|User), UpdatedAt e índices.
-- Ejecutar en PostgreSQL (Supabase SQL Editor o psql) si aplicás cambios fuera de dotnet ef.

-- FullName opcional
ALTER TABLE "Users" ALTER COLUMN "FullName" DROP NOT NULL;

-- IsActive por defecto true (idempotente si ya existe el default)
ALTER TABLE "Users" ALTER COLUMN "IsActive" SET DEFAULT true;

-- UpdatedAt: columna NOT NULL, backfill desde CreatedAt
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamptz NOT NULL DEFAULT timezone('utc', now());
UPDATE "Users" SET "UpdatedAt" = "CreatedAt";

-- Normalizar Role antes del CHECK (solo 'Admin' | 'User')
UPDATE "Users" SET "Role" = 'Admin'
WHERE UPPER(TRIM(COALESCE("Role", ''))) IN ('ADMIN', 'ADMINISTRATOR');
UPDATE "Users" SET "Role" = 'User'
WHERE UPPER(TRIM(COALESCE("Role", ''))) IN ('USER', '');
UPDATE "Users" SET "Role" = 'User'
WHERE "Role" NOT IN ('Admin', 'User');

ALTER TABLE "Users" DROP CONSTRAINT IF EXISTS "CK_Users_Role";
ALTER TABLE "Users" ADD CONSTRAINT "CK_Users_Role" CHECK ("Role" IN ('Admin', 'User'));

-- Índices (Email único y BusinessId suelen existir ya vía EF)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_BusinessId" ON "Users" ("BusinessId");
