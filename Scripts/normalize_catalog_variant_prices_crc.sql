-- Normaliza "CatalogVariants"."Price" al estándar CRC: múltiplo de 5 colones (ceil tras ROUND a 2 decimales).
-- Ejecutar en staging primero; respaldo recomendado.
--
-- Equivalente a CrcSalePriceNormalizer.NormalizeSalePriceColones en Domain/Pricing/CrcSalePriceNormalizer.cs

UPDATE "CatalogVariants" AS v
SET "Price" = s.normalized
FROM (
    SELECT
        "Id",
        CASE
            WHEN n <= 0 THEN 0::numeric
            WHEN MOD(n, 5::numeric) = 0 THEN n
            ELSE CEIL(n / 5::numeric) * 5
        END AS normalized
    FROM (
        SELECT "Id", ROUND("Price"::numeric, 2) AS n
        FROM "CatalogVariants"
    ) x
) AS s
WHERE v."Id" = s."Id"
  AND v."Price" IS DISTINCT FROM s.normalized;
