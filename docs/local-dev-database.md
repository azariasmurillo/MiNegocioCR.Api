# Base de datos local (PostgreSQL)

## Setup inicial

```powershell
cd MiNegocioCR.Api
dotnet ef database update
```

Con los `.Designer.cs` registrados (mayo 2026), **`dotnet ef database update` aplica todas las migraciones**, incluidas las de descuentos en ventas y la eliminación de `DiscountPercent` en reparaciones.

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

Deben existir **5 columnas** listadas y la tabla `SalePaymentMethods`.

**RepairOrders:** la columna `DiscountPercent` **no debe existir** (descuento solo en `Sales` al facturar).

```sql
SELECT column_name FROM information_schema.columns
WHERE table_name = 'RepairOrders' AND column_name = 'DiscountPercent';
-- Esperado: 0 filas
```

## Migraciones críticas (ventas + dashboard)

| Migración | Qué agrega / quita |
|-----------|---------------------|
| `20260504220000_AddSaleCostAndProfitMetrics` | `TotalCost`, `TotalProfit`, `SaleItems.CostPrice` |
| `20260522120000_RefactorPaymentsAndSalePaymentMethods` | Tabla `SalePaymentMethods`, `TotalOrden`, `PrepaidAmount` |
| `20260526120000_RemoveRepairOrderDiscountPercent` | **Quita** `DiscountPercent` de `RepairOrders` |
| `20260526130000_AddSaleDiscountMetadata` | `DiscountKind`, `DiscountInputValue` en `Sales` |

Sin estas columnas/tablas, fallan **dashboard** y **POST /api/sales** con HTTP 500.

### Migraciones CRM y campañas (mayo 2026)

| Migración | Qué agrega |
|-----------|------------|
| `20260527120000_AddContactLastActivityAt` | `Contacts.LastActivityAt` |
| `20260528120000_AddContactEmailCampaign` | `LastMarketingEmailAt`, tabla `ContactEmailCampaignLogs` |
| `20260529120000_AddEmailCampaignQueue` | `EmailCampaigns`, `EmailCampaignRecipients` |

Sin estas tablas/columnas, fallan **Clientes → Campaña de correo** y el worker de cola.

Guía funcional: [email-campaigns-crm.md](./email-campaigns-crm.md).  
Emergencia: `Scripts/cancel-active-campaigns.sql`.

### Columna legacy `TotalAmount`

- Existe desde antes del refactor; **NOT NULL** en PostgreSQL.
- Debe persistirse igual que `Total` (monto cobrado hoy). Con saldo ₡0, EF debe enviar `0`, no omitir la columna (ver FIXES §25).
- Al arrancar, `AppDbContext.SaveChangesAsync` sincroniza `TotalAmount = Total`.

### Test de integración PostgreSQL

```powershell
cd MiNegocioCR.Api
dotnet test --filter RegisterSalePostgresIntegrationTests
```

Requiere PostgreSQL local (`MiNegocioCR_Dev` en `appsettings.Development.json`). Si la BD no está disponible, el test se omite. **Recomendado** antes de cambiar `RegisterSaleUseCase` o mapeo de `Sales`.

### SQL manual — quitar descuento de reparaciones

Si EF no aplica la migración `20260526120000`:

```sql
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "DiscountPercent";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260526120000_RemoveRepairOrderDiscountPercent', '8.0.8'
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory"
  WHERE "MigrationId" = '20260526120000_RemoveRepairOrderDiscountPercent'
);
```

Script completo: `Scripts/apply-pending-migrations.sql`

## Auto-migración al arrancar

La API aplica migraciones pendientes al iniciar (`APPLY_MIGRATIONS_ON_STARTUP=true` por defecto). En local, conviene correr `dotnet ef database update` **antes** del primer `dotnet run` para ver errores de schema en la terminal y no en el navegador.

## Contactos y NoTracking

El API usa `QueryTrackingBehavior.NoTracking` global. Al reutilizar contactos existentes (ventas u órdenes de reparación), los helpers deben usar `.AsTracking()` y las entidades nuevas solo deben setear `ContactId` (no la navegación `Contact`). Ver:

- `Application/Common/SaleContactResolution.cs`
- `Application/Common/RepairOrderContactHelper.cs`
- Changelog: `docs/FIXES_MAYO_2026.md` (copia) o `FIXES_MAYO_2026.md` en la raíz del workspace

## Tests

```powershell
dotnet test
```

Esperado: **160+** tests pasando (incluye campañas CRM; mayo 2026).

## Documentación relacionada

| Archivo | Contenido |
|---------|-----------|
| `docs/DEPLOY.md` | Deploy producción (Railway, Vercel, Supabase) |
| `docs/FIXES_MAYO_2026.md` | Changelog de fixes |
| `docs/email-campaigns-crm.md` | Campañas de correo en cola |
| `Scripts/verify-schema.sql` | Verificación post-migración |
| `Scripts/apply-pending-migrations.sql` | Respaldo idempotente |

*Última actualización: 25 mayo 2026*
