-- Tabla de recuperación de contraseña (token en columna "Token" = hash SHA-256 en hex del valor enviado por email).
-- Válido con el modelo actual de la API. Ejecutar en PostgreSQL si no usás dotnet ef database update.

CREATE TABLE IF NOT EXISTS "PasswordResetTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Token" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsUsed" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PasswordResetTokens_Users_UserId"
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_PasswordResetTokens_Token"
    ON "PasswordResetTokens" ("Token");

CREATE INDEX IF NOT EXISTS "IX_PasswordResetTokens_UserId"
    ON "PasswordResetTokens" ("UserId");
