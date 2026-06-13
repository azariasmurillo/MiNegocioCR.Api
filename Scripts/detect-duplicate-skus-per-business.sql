-- Detecta SKUs duplicados dentro del mismo negocio (case-insensitive).
-- Ejecutar ANTES de aplicar migración AddUniqueSkuPerBusiness.
-- Corregir manualmente los duplicados listados antes de dotnet ef database update.

SELECT
    ci."BusinessId",
    LOWER(TRIM(v."SKU")) AS sku_key,
    COUNT(*) AS variant_count,
    STRING_AGG(v."Id"::text, ', ' ORDER BY v."CreatedAt") AS variant_ids,
    STRING_AGG(ci."Name", ' | ' ORDER BY v."CreatedAt") AS product_names
FROM "CatalogVariants" v
INNER JOIN "CatalogItems" ci ON ci."Id" = v."CatalogItemId"
WHERE v."SKU" IS NOT NULL AND TRIM(v."SKU") <> ''
GROUP BY ci."BusinessId", LOWER(TRIM(v."SKU"))
HAVING COUNT(*) > 1
ORDER BY variant_count DESC, sku_key;
