-- Se ejecuta SOLO si "dotnet ef database update" falla al crear
--   IX_RepairOrders_BusinessId_OrderNumber
-- con el error: 23505 (valores duplicados).
--
-- Tras el ALTER a texto (lpad 6) pueden quedar el mismo (BusinessId, OrderNumber) en
-- varias filas. Este script lista duplicados y, si hace falta, los desempata.

-- 1) Ver duplicados
SELECT "BusinessId", "OrderNumber", COUNT(*) AS cnt
FROM "RepairOrders"
GROUP BY "BusinessId", "OrderNumber"
HAVING COUNT(*) > 1;

-- 2) Hacer únicos: para cada fila “extra” de un mismo par, anexar un sufijo único
--    (mantiene longitud bajo 32 y evita chocar con otras; ajusta si hace falta)
UPDATE "RepairOrders" ro
SET "OrderNumber" = "OrderNumber" || '_' || replace(ro."Id"::text, '-', '')
FROM (
    SELECT "Id",
           ROW_NUMBER() OVER (
             PARTITION BY "BusinessId", "OrderNumber" ORDER BY "CreatedAt" ASC, "Id" ASC
           ) AS seq
    FROM "RepairOrders"
) x
WHERE ro."Id" = x."Id" AND x.seq > 1;

-- 3) Vuelve a correr: dotnet ef database update --project MiNegocioCR.Api.csproj --context AppDbContext
