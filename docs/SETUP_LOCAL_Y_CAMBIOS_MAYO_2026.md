# Setup local + cambios recientes (Mayo 2026)

**Repositorio:** `c:\Mi-negociocr`  
**Contexto:** sesión de onboarding a PostgreSQL local, correos, auth y fixes de dashboard/UI.  
**Complementa:** `REFACTOR_REPAIR_PAYMENTS_SALES.md` (refactor financiero de reparaciones/ventas).  
**Changelog:** `FIXES_MAYO_2026.md` (fixes detallados). **Deploy:** `DEPLOY.md`.  
**Jun 2026:** Pedidos Internet, Créditos y layout responsive — ver §15–16.

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [PostgreSQL local](#2-postgresql-local)
3. [Datos iniciales JoyCaTech](#3-datos-iniciales-joycatech)
4. [Correos (Resend)](#4-correos-resend)
5. [Recuperación de contraseña — bug y fix](#5-recuperación-de-contraseña--bug-y-fix)
6. [Dashboard 500 — schema desactualizado](#6-dashboard-500--schema-desactualizado)
7. [UI reparaciones — sección facturación removida](#7-ui-reparaciones--sección-facturación-removida)
8. [Frontend dev vs producción](#8-frontend-dev-vs-producción)
9. [Notas operativas](#9-notas-operativas)
10. [Archivos tocados](#10-archivos-tocados)
11. [Contactos repetidos — NoTracking](#11-contactos-repetidos--notracking)
12. [WhatsApp panel recogido](#12-whatsapp-panel-recogido)
13. [Deploy mayo 2026](#13-deploy-mayo-2026)
14. [CRM — campañas de correo](#14-crm--campañas-de-correo)
15. [Junio 2026 — Pedidos Internet y Créditos (docs)](#15-junio-2026--pedidos-internet-y-créditos-docs)
16. [Junio 2026 — Layout responsive (menú colapsable)](#16-junio-2026--layout-responsive-menú-colapsable)

---

## 1. Resumen ejecutivo

| Tema | Qué pasó | Estado |
|------|----------|--------|
| BD local | PostgreSQL 18 en `localhost:5432`, base `MiNegocioCR_Dev` | ✅ |
| Conexión dev | `appsettings.Development.json` + override en `launchSettings.json` | ✅ |
| Tenant demo | Negocio **JoyCaTech** + usuario admin | ✅ |
| Emails | Resend en dev (`appsettings.Development.json`) y Railway (`Resend__ApiKey`) | ✅ |
| Reset password | Bug EF `NoTracking` — contraseña no se guardaba | ✅ Fix |
| Dashboard 500 | Faltaban columnas/tablas por migraciones huérfanas | ✅ Fix + script |
| Gestionar orden | Removida sección “Datos de facturación” / descuento editable | ✅ |
| Contacto repetido | 500 `PK_Contacts` en ventas y órdenes de reparación | ✅ Fix |
| Email factura | Correo vacío en factura / envío por email | ✅ Fix |
| WhatsApp UI | Panel lateral recogido por defecto | ✅ |

---

## 2. PostgreSQL local

### Connection string (desarrollo)

```
Host=localhost;Port=5432;Database=MiNegocioCR_Dev;Username=postgres;Password=postgres
```

### Dónde está configurada

| Archivo | Propósito |
|---------|-----------|
| `MiNegocioCR.Api/appsettings.Development.json` | `ConnectionStrings:DefaultConnection` |
| `MiNegocioCR.Api/Properties/launchSettings.json` | Perfiles `http` y `https`: `POSTGRES_CONNECTION_STRING` y `ConnectionStrings__DefaultConnection` |

**Importante:** en Windows existía una variable de **sistema** `ConnectionStrings__DefaultConnection` apuntando a Supabase que **pisaba** el appsettings. El `launchSettings.json` fuerza localhost al correr desde Visual Studio / `dotnet run --launch-profile https`.

### Migraciones EF

```bash
cd MiNegocioCR.Api
dotnet ef database update
```

Luego ejecutar el script de schema pendiente (ver sección 6):

```bash
psql -U postgres -d MiNegocioCR_Dev -f scripts/apply-pending-migrations.sql
```

Documentación detallada: `MiNegocioCR.Api/docs/local-dev-database.md`

### Cómo correr en local

```bash
# Terminal 1 — API (perfil HTTPS para el proxy del frontend)
cd MiNegocioCR.Api
dotnet run --launch-profile https

# Terminal 2 — Frontend
cd mi-negociocr-frontend
npm start
```

- API: `https://localhost:7176`
- Frontend: `http://localhost:4200` (proxy → API)

**Regla:** no dejar la API corriendo en background desde Cursor si vas a compilar en Visual Studio (bloquea `MiNegocioCR.Api.exe`).

---

## 3. Datos iniciales JoyCaTech

Tenant mínimo insertado en `MiNegocioCR_Dev`:

| Campo | Valor |
|-------|-------|
| Negocio | JoyCaTech |
| Business ID | `f46f721d-83f2-4a2f-a003-05b3ed4dd5bb` |
| Email | `joycatech@gmail.com` |
| Contraseña (activa) | `JoyCaTech2026!` |
| Rol | Admin |

Tablas mínimas: `Businesses`, `BusinessSettings`, `Users`.

---

## 4. Correos (Resend)

### Desarrollo

Configurado en `MiNegocioCR.Api/appsettings.Development.json`:

```json
"Resend": {
  "ApiKey": "<ver archivo — no commitear en repos públicos>",
  "FromEmail": "no-replay@mi-negociocr.com",
  "FromName": "MiNegocioCR"
}
```

Servicio activo: `ResendEmailService` (envío real).

`ConsoleEmailService` existe en `Infrastructure/Services/ConsoleEmailService.cs` para imprimir links en consola si algún día se quiere dev sin Resend (registrar condicionalmente en `Program.cs`).

### Producción (Railway)

Variable requerida en **Railway** (no en Vercel — el email lo manda el backend):

```
Resend__ApiKey = re_...
```

El doble guión bajo `__` mapea a `Resend:ApiKey` en .NET.

### Enlaces de reset

`App:PublicUrl` en dev = `http://localhost:4200` → los correos llevan link a `/reset-password?token=...`

En producción, `App__PublicUrl` en Railway debe ser la URL real del frontend (ej. `https://mi-negociocr.com`).

---

## 5. Recuperación de contraseña — bug y fix

### Síntoma

- El correo llegaba y la UI decía “Contraseña actualizada”.
- Al hacer login, la **contraseña vieja** seguía funcionando.

### Causa

`AppDbContext` usa `QueryTrackingBehavior.NoTracking` global. En `PasswordResetTokenRepository.TryCompletePasswordResetAsync` se modificaba `User.PasswordHash` sobre entidades **no trackeadas** → `SaveChanges()` no persistía nada.

### Fix

`Infrastructure/Persistence/Repositories/PasswordResetTokenRepository.cs`:

```csharp
var row = await _context.PasswordResetTokens
    .AsTracking()  // ← agregado
    .Include(x => x.User)
    ...
```

### Frontend

`reset-password.ts` / `.html`: mensaje de error si el POST falla (antes quedaba en silencio).

### Token

- Duración: **10 minutos** (`RequestPasswordResetUseCase`).
- Pedir enlace nuevo después de deployar el fix.

---

## 6. Dashboard 500 — schema desactualizado

### Síntoma

Errores 500 en consola del navegador:

- `GET /api/dashboard/{id}/profit-by-source`
- `GET /api/dashboard/{id}/sales-trend?...`
- `GET /api/sales/business/{id}?page=1&pageSize=10`

**No era falta de datos.** Con BD vacía deberían responder 200 con ceros/listas vacías.

### Causa

Dos migraciones existían en código pero **sin `.Designer.cs`**, por lo que `dotnet ef database update` no las aplicaba:

| Migración | Qué agrega |
|-----------|------------|
| `20260504220000_AddSaleCostAndProfitMetrics` | `Sales.TotalProfit`, `TotalCost`, `SaleItems.CostPrice` |
| `20260522120000_RefactorPaymentsAndSalePaymentMethods` | tabla `SalePaymentMethods`, `TotalOrden`, `PrepaidAmount` |

El código ya consultaba esas columnas → PostgreSQL error → 500.

### Fix aplicado en local

Script idempotente:

```
MiNegocioCR.Api/scripts/apply-pending-migrations.sql
```

También inserta filas en `__EFMigrationsHistory` para esas dos migraciones.

### Frontend

`dashboard.ts`: `catchError` en `listSales()` para no romper la UI si el API falla (muestra lista vacía).

### Producción

Si Supabase/Railway muestra los mismos 500, verificar que el schema tenga `TotalProfit`, `TotalOrden`, `PrepaidAmount` y la tabla `SalePaymentMethods`. Aplicar el mismo script adaptado o correr migraciones pendientes allí.

---

## 7. UI reparaciones — sección facturación removida

Según `REFACTOR_REPAIR_PAYMENTS_SALES.md`, en **Gestionar orden** no debe haber:

- Sección “Datos de facturación”
- Campo editable “Descuento (%)”
- Texto sobre métodos de pago al facturar (eso es en POS)

### Cambio

`mi-negociocr-frontend/.../repair-order-edit-dialog.ts`:

- Eliminado bloque `billing-section` completo.
- Removido `discountPercent` del formulario y del payload de guardado (no se pisa el valor en BD al editar otros campos).

### Qué se mantiene

- Sección **Pagos** con resumen financiero (subtotal, descuento *solo si existe en BD*, IVA, total, abonado, saldo).
- Registro de abonos vía `Registrar pago`.
- Al facturar: `/sales?repairOrderId=...` (POS readonly; métodos de pago ahí).

---

## 8. Frontend dev vs producción

| Modo | Config | API destino |
|------|--------|-------------|
| `npm start` | `environment.ts` + `proxy.conf.json` | `https://localhost:7176` |
| Build prod | `environment.prod.ts` | Railway |

No hay toggle en la UI: el proxy de Angular CLI redirige `/api` y `/chatHub` al backend local en desarrollo.

---

## 9. Notas operativas

### Error de consola del navegador (extensiones)

```
Uncaught (in promise) Error: A listener indicated an asynchronous response...
```

Casi siempre es una **extensión de Chrome** (Grammarly, password managers, ad blockers), no código de MiNegocioCR. Probar en incógnito para confirmar.

### JWT en desarrollo

`Jwt:Key` en `appsettings.Development.json` — solo para dev local. Producción usa `Jwt__Key` en Railway.

### Credenciales sensibles

- No subir API keys de Resend a repos públicos.
- Rotar keys si se expusieron en chat/commits.

### Deploy pendiente a Railway / Vercel

Ver checklist completo en [`DEPLOY.md`](./DEPLOY.md). Resumen:

1. `dotnet test` (140 tests) + `npm run build`
2. Commit + push API (`master`) y frontend (`main`)
3. Migraciones Supabase prod (5432) + `verify-schema.sql`
4. Redeploy Railway + Vercel
5. Smoke test (contacto repetido, ventas con descuento, email factura)

Asegurar que estén desplegados:

1. Fix `AsTracking` en reset de contraseña, estados de orden, **contactos** (ventas + reparaciones).
2. Variable `Resend__ApiKey` y `App__PublicUrl`.
3. Schema de BD al día (migraciones / script).
4. Frontend: WhatsApp recogido, pagos en carrito, Completar restante.

---

## 10. Archivos tocados

### Backend (`MiNegocioCR.Api`)

| Archivo | Cambio |
|---------|--------|
| `appsettings.Development.json` | BD local, JWT, Resend, App URL |
| `Properties/launchSettings.json` | Override connection string localhost |
| `Infrastructure/Services/ConsoleEmailService.cs` | Nuevo (opcional dev sin email) |
| `Infrastructure/Persistence/Repositories/PasswordResetTokenRepository.cs` | `.AsTracking()` en reset |
| `Application/UseCases/Sales/RegisterSaleUseCase.cs` | Contacto ventas: solo `ContactId`, `.AsTracking()` |
| `Application/Common/SaleContactResolution.cs` | `.AsTracking()` al reutilizar contacto |
| `Application/Common/RepairOrderContactHelper.cs` | `.AsTracking()` + fix órdenes reparación |
| `scripts/apply-pending-migrations.sql` | Schema pendiente |
| `Scripts/verify-schema.sql` | Verificación schema |
| `docs/local-dev-database.md` | Guía BD local |
| `docs/DEPLOY.md` | Copia guía deploy (respaldo) |
| `docs/FIXES_MAYO_2026.md` | Copia changelog (respaldo) |

### Frontend (`mi-negociocr-frontend`)

| Archivo | Cambio |
|---------|--------|
| `features/repairs/components/repair-order-edit-dialog.ts` | Quitada sección facturación/descuento |
| `features/auth/pages/reset-password/*` | Mensaje de error en fallo |
| `features/dashboard/pages/dashboard/dashboard.ts` | catchError en ventas |
| `layout/whatsapp-panel/whatsapp-panel.ts` | Panel WhatsApp recogido por defecto |
| `layout/layout-shell/layout-shell.scss` | Panel visible en todas las resoluciones desktop |

---

## 11. Contactos repetidos — NoTracking

### Problema

Con `QueryTrackingBehavior.NoTracking` global (`Program.cs`), reutilizar un contacto existente (mismo teléfono o `contactId`) podía provocar:

```
23505: duplicate key value violates unique constraint "PK_Contacts"
```

Ocurría al **crear orden de reparación** o al **facturar una venta**.

### Regla de oro

- Buscar contacto con `.AsTracking()` en helpers (`SaleContactResolution`, `RepairOrderContactHelper`).
- Al crear `Sale` u `RepairOrder`: asignar solo **`ContactId`**, nunca la navegación `Contact = contact`.

### Archivos clave

| Área | Archivos |
|------|----------|
| Ventas | `SaleContactResolution.cs`, `RegisterSaleUseCase.cs`, `GetSaleByIdUseCase.cs` |
| Reparaciones | `RepairOrderContactHelper.cs`, `CreateRepairOrderUseCase.cs`, `UpdateRepairOrderUseCase.cs` |
| Tests | `RegisterSaleUseCaseTests.cs`, `RepairOrderContactHelperTests.cs` |

Detalle: [`FIXES_MAYO_2026.md`](./FIXES_MAYO_2026.md) §18 y §19.

---

## 12. WhatsApp panel recogido

El panel lateral de mensajes WhatsApp **no se oculta**; arranca **recogido** (botón 💬, ~64px). El usuario expande con clic y cierra con ◀.

| Archivo | Cambio |
|---------|--------|
| `whatsapp-panel.ts` | `collapsed = true` por defecto |
| `layout-shell.scss` | Sin ocultar panel en pantallas &lt; 1360px |

Las conversaciones siguen cargándose al iniciar la app. Ver [`FIXES_MAYO_2026.md`](./FIXES_MAYO_2026.md) §20.

---

## 13. Deploy mayo 2026

Documentación de deploy y changelog viven en la raíz del workspace `Mi-negociocr/`:

| Archivo | Uso |
|---------|-----|
| `DEPLOY.md` | Guía Railway + Vercel + Supabase |
| `FIXES_MAYO_2026.md` | Historial de correcciones |
| `REFACTOR_REPAIR_PAYMENTS_SALES.md` | Modelo financiero |

Copias de respaldo en `MiNegocioCR.Api/docs/` para incluir en Git del repo API.

---

## 14. CRM — campañas de correo

Guía completa: **`MiNegocioCR.Api/docs/email-campaigns-crm.md`**.

### Local

1. `dotnet ef database update` (incluye `20260529120000_AddEmailCampaignQueue`).
2. Resend configurado en `appsettings.Development.json` (misma key que prod o dominio de prueba).
3. Frontend: `ng serve` → **Clientes** → **Campaña de correo**.

### Probar sin spamear

- Audiencia pequeña (preview).
- Asunto ≥8 caracteres; cuerpo ≥25 (o ≥10 con imagen).
- Tras encolar: ~**1 correo por minuto**; status en UI cada 10 s.
- **Detener campaña** si necesitás cortar.

### Emergencia

`MiNegocioCR.Api/Scripts/cancel-active-campaigns.sql` en Supabase SQL Editor.

Changelog de bugs (re-envío, dedupe): `FIXES_MAYO_2026.md` §26–28.

---

## 15. Junio 2026 — Pedidos Internet y Créditos (docs)

| Módulo | Estado | Documento |
|--------|--------|-----------|
| Pedidos Internet | Implementado (API + frontend) | [PEDIDOS_INTERNET_DISENO_v1.md](./PEDIDOS_INTERNET_DISENO_v1.md), [CAMBIOS_PEDIDOS_INTERNET_JUNIO_2026.md](./CAMBIOS_PEDIDOS_INTERNET_JUNIO_2026.md) |
| Créditos / cuentas por cobrar | Implementado (API + frontend) | [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./CREDITOS_CUENTAS_COBRAR_DISENO_v1.md), [CAMBIOS_CREDITOS_JUNIO_2026.md](./CAMBIOS_CREDITOS_JUNIO_2026.md) |

Migración Pedidos Internet: `20260604142659_AddInternetOrders` (ver `apply-pending-migrations.sql` §8 y [DEPLOY.md](./DEPLOY.md)).

Créditos: una cuenta corriente por `Contact`, historial inmutable `CreditTransaction`, recordatorios **manuales** (sin cron). Tenant = `BusinessId`.

---

## 16. Junio 2026 — Layout responsive (menú colapsable)

| Tema | Estado | Documento |
|------|--------|-----------|
| Menú lateral ocultable (desktop + drawer móvil) | ✅ Frontend | [CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md](./CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md) |
| Tarjeta «Workspace» removida del sidebar | ✅ | mismo doc |

**Solo frontend** — deploy en Vercel al push de `main`. Sin migraciones ni Railway.

Servicio: `LayoutShellService` (`src/app/layout/layout-shell.service.ts`). Preferencia: `localStorage` clave `mnr-sidebar-hidden`.

---

## Referencias cruzadas

- Refactor financiero: `REFACTOR_REPAIR_PAYMENTS_SALES.md`
- Fixes y deploy: `FIXES_MAYO_2026.md`, `DEPLOY.md`
- BD local: [local-dev-database.md](./local-dev-database.md)
- Backend auth: `API/Controllers/AuthController.cs`
- Reset password: `Application/UseCases/Auth/ResetPasswordUseCase.cs`

---

*Última actualización: 6 junio 2026 — Layout responsive; Créditos implementado; Pedidos Internet*
