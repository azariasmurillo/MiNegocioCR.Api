# Base de datos local (PostgreSQL)

## Setup inicial

```powershell
cd MiNegocioCR.Api
dotnet ef database update
```

Con los `.Designer.cs` registrados (mayo 2026), **`dotnet ef database update` aplica todas las migraciones**, incluidas las de descuentos en ventas.

### Script de respaldo (opcional)

Si la BD quedó a medias en una sesión anterior, el script idempotente sigue siendo útil:

```powershell
# Requiere psql en PATH
psql -U postgres -d MiNegocioCR_Dev -f Scripts/apply-pending-migrations.sql
```

## Verificar schema

```powershell
psql -U postgres -d MiNegocioCR_Dev -f Scripts/verify-schema.sql
```

O manualmente:

```sql
SELECT column_name FROM information_schema.columns
WHERE table_name = 'Sales'
  AND column_name IN (
    'TotalProfit', 'TotalOrden', 'PrepaidAmount',
    'DiscountKind', 'DiscountInputValue'
  );
```

Deben existir **5 columnas** y la tabla `SalePaymentMethods`.

## Migraciones críticas (ventas + dashboard)

| Migración | Qué agrega |
|-----------|------------|
| `20260504220000_AddSaleCostAndProfitMetrics` | `TotalCost`, `TotalProfit`, `SaleItems.CostPrice` |
| `20260522120000_RefactorPaymentsAndSalePaymentMethods` | Tabla `SalePaymentMethods`, `TotalOrden`, `PrepaidAmount` |
| `20260526120000_RemoveRepairOrderDiscountPercent` | Quita `DiscountPercent` de `RepairOrders` |
| `20260526130000_AddSaleDiscountMetadata` | `DiscountKind`, `DiscountInputValue` en `Sales` |

Sin estas columnas/tablas, fallan **dashboard** y **POST /api/sales** con HTTP 500.

## Auto-migración al arrancar

La API aplica migraciones pendientes al iniciar (`APPLY_MIGRATIONS_ON_STARTUP=true` por defecto). En local, conviene correr `dotnet ef database update` **antes** del primer `dotnet run` para ver errores de schema en la terminal y no en el navegador.
