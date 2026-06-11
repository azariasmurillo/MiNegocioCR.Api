# Deploy a producción — MiNegocioCR

Guía para desplegar el monorepo actual sin sorpresas en producción.

**Stack objetivo:** Railway (API .NET 8) + Vercel (Angular 21) + Supabase (PostgreSQL + Storage).

**Complementa:** [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) (setup local y fixes recientes).  
**Changelog:** [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) (registro detallado de correcciones).

---

## Índice

1. [Arquitectura](#1-arquitectura)
2. [Pre-requisitos](#2-pre-requisitos)
3. [Checklist antes de deploy](#3-checklist-antes-de-deploy)
4. [Paso 1 — Subir código](#4-paso-1--subir-código)
5. [Paso 2 — Variables de entorno (Railway)](#5-paso-2--variables-de-entorno-railway)
6. [Paso 3 — Base de datos (crítico)](#6-paso-3--base-de-datos-crítico)
7. [Paso 4 — Deploy API (Railway)](#7-paso-4--deploy-api-railway)
8. [Paso 5 — Deploy frontend (Vercel)](#8-paso-5--deploy-frontend-vercel)
9. [Paso 6 — Smoke test post-deploy](#9-paso-6--smoke-test-post-deploy)
10. [Riesgos conocidos](#10-riesgos-conocidos)
11. [Orden recomendado (resumen)](#11-orden-recomendado-resumen)
12. [Documentación del proyecto](#12-documentación-del-proyecto)
13. [Referencias](#13-referencias)

---

## 1. Arquitectura

```
Usuario → Vercel (Angular) → Railway (API .NET 8)
                                  ├── Supabase PostgreSQL
                                  ├── Supabase Storage (logos, fotos, catálogo)
                                  ├── Resend (correos transaccionales)
                                  ├── Meta / WhatsApp (opcional)
                                  └── OpenAI (opcional, chat IA)
```

| Componente | Repo / carpeta | Hosting |
|------------|----------------|---------|
| API | `MiNegocioCR.Api/` | Railway |
| Frontend | `mi-negociocr-frontend/` | Vercel |
| Base de datos | — | Supabase Postgres |
| Archivos | — | Supabase Storage (`business-assets`) |

---

## 2. Pre-requisitos

- Cuentas activas en **Railway**, **Vercel** y **Supabase**
- Dominio verificado en **Resend** (`no-replay@mi-negociocr.com` o el remitente que uses)
- .NET 8 SDK y Node.js en la máquina local (para builds y migraciones)
- Acceso a la connection string de Supabase (**puerto 5432 directo** para migraciones)

---

## 3. Checklist antes de deploy

Ejecutar en local antes de subir:

```bash
# API — tests
cd MiNegocioCR.Api
dotnet test

# Frontend — build producción
cd ../mi-negociocr-frontend
npm run build
```

| Verificación | Esperado |
|--------------|----------|
| Tests API | **204** pasando (detener API local si bloquea `MiNegocioCR.Api.exe` durante `dotnet test`) |
| Build frontend | Sin errores TypeScript (warnings de budget CSS OK) |
| Cambios commiteados | API (`master`) y frontend (`main`) |
| Variables Railway | Todas las obligatorias configuradas |
| Schema BD prod | Migraciones + script pendiente aplicados |

---

## 4. Paso 1 — Subir código

El proyecto usa **dos repositorios Git** separados:

| Repo | Rama | Carpeta |
|------|------|---------|
| API | `master` | `MiNegocioCR.Api/` |
| Frontend | `main` | `mi-negociocr-frontend/` |

Railway y Vercel despliegan desde GitHub. Sin **commit + push** en ambos repos, producción no recibe los cambios nuevos.

Cambios recientes que deben estar incluidos en el deploy:

- Fix `.AsTracking()` en estados de orden y ventas
- **Contacto repetido:** ventas (`RegisterSaleUseCase`, `SaleContactResolution`) y reparaciones (`RepairOrderContactHelper`) — evita `PK_Contacts`
- **Email en factura:** `GetSaleById` + mappers frontend
- Ítems enriquecidos en detalle de orden
- Vista previa y correo con HTML dedicado (no iframe)
- Descuentos solo en venta (`DiscountKind`, `DiscountInputValue`); **sin** `DiscountPercent` en `RepairOrders`
- Migraciones mayo 2026 con `.Designer.cs` + script idempotente
- Script `MiNegocioCR.Api/Scripts/apply-pending-migrations.sql` (respaldo)
- Script `MiNegocioCR.Api/Scripts/verify-schema.sql` (comprobar schema)
- Frontend: WhatsApp panel **recogido por defecto** (`collapsed = true`)
- Frontend: pagos al pie del carrito, efectivo precargado, **Completar restante**
- **CRM campañas de correo:** cola global (`EmailCampaigns`), worker 60 s, cupo 495/día, cancelación, dedupe por email, validaciones de contenido — ver `MiNegocioCR.Api/docs/email-campaigns-crm.md`
- Fix **re-envío en bucle** de campaña (`CampaignQueueProcessor`, progreso desde recipients)
- **Inventario Sprint 4 (Jun 2026):**
  - API: `PATCH /variants/{id}/toggle`, `ProfitMargin`/`IsActive`/`PrimaryImageUrl` en listados — ver [CAMBIOS_INVENTARIO_API_JUNIO_2026.md](./CAMBIOS_INVENTARIO_API_JUNIO_2026.md)
  - FE: grid responsive `/inventory`, filtro Inactivos, fix producto/servicio — ver [CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md](../mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md)
  - **Sin migraciones nuevas** de BD para este release de inventario

---

## 5. Paso 2 — Variables de entorno (Railway)

Configurar en el servicio de la API en Railway. En .NET, el doble guión bajo `__` equivale a `:` en JSON (`Jwt__Key` → `Jwt:Key`).

### Obligatorias

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno ASP.NET | `Production` |
| `POSTGRES_CONNECTION_STRING` | PostgreSQL (Supabase) | Ver Supabase → Database → Connection string (direct, port 5432) |
| `Jwt__Key` | Clave JWT (larga, aleatoria, **distinta a dev**) | Generar con herramienta segura |
| `Admin__Password` | Panel `/admin` — **sin esto la API no arranca** | Contraseña fuerte |
| `Resend__ApiKey` | API key de Resend | `re_...` |
| `Resend__FromEmail` | Remitente verificado | `no-replay@mi-negociocr.com` |
| `Resend__FromName` | Nombre del remitente (opcional) | `MiNegocioCR` |
| `App__PublicUrl` | URL pública del **frontend** (links reset password) | `https://mi-negociocr.com` |
| `SUPABASE_URL` | URL del proyecto Supabase | `https://xxxx.supabase.co` |
| `SUPABASE_SERVICE_ROLE_KEY` | Service role key (storage) | Desde Supabase → Settings → API |

Alternativas aceptadas por el código:

- `RESEND_API_KEY` en lugar de `Resend__ApiKey`
- `Supabase:Url` / `Supabase:ServiceKey` vía variables `Supabase__Url`, `Supabase__ServiceKey`
- `ConnectionStrings__DefaultConnection` si no usas `POSTGRES_CONNECTION_STRING`

### Opcionales (solo si usas la función)

| Variable | Descripción |
|----------|-------------|
| `WhatsApp__AppId` | App ID Meta |
| `WhatsApp__AppSecret` | App Secret Meta |
| `Meta__RedirectUri` | `https://<tu-api-railway>/api/whatsapp/oauth-callback` |
| `OpenAI__ApiKey` | Chat IA |

### Correos en producción

- Se usa **`ResendEmailService`** (envío real).
- `ConsoleEmailService` existe solo para desarrollo local; no está registrado en producción.
- Documentación adicional: `MiNegocioCR.Api/RESEND_SETUP.md`

---

## 6. Paso 3 — Base de datos (crítico)

**Causa #1 de errores 500 en producción:** schema incompleto (columnas/tablas que el código ya usa).

### Migraciones que deben estar aplicadas

| Migración | Qué agrega |
|-----------|------------|
| `20260504220000_AddSaleCostAndProfitMetrics` | `Sales.TotalProfit`, `TotalCost`, `SaleItems.CostPrice` |
| `20260522120000_RefactorPaymentsAndSalePaymentMethods` | Tabla `SalePaymentMethods`, `TotalOrden`, `PrepaidAmount` |
| `20260526120000_RemoveRepairOrderDiscountPercent` | Elimina `DiscountPercent` de `RepairOrders` |
| `20260526130000_AddSaleDiscountMetadata` | `DiscountKind`, `DiscountInputValue` en `Sales` |
| `20260527120000_AddContactLastActivityAt` | `Contacts.LastActivityAt` |
| `20260528120000_AddContactEmailCampaign` | `LastMarketingEmailAt`, `ContactEmailCampaignLogs` |
| `20260529120000_AddEmailCampaignQueue` | `EmailCampaigns`, `EmailCampaignRecipients` |
| `20260604142659_AddInternetOrders` | `InternetOrders`, líneas y abonos (Pedidos Internet) |

**Pendiente (sin migración aún):** módulo Créditos — ver [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./MiNegocioCR.Api/docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md).

Desde mayo 2026 estas migraciones tienen `[Migration]` / `.Designer.cs` y **EF las detecta**. `dotnet ef database update` y el arranque de la API (`APPLY_MIGRATIONS_ON_STARTUP=true`, default) las aplican.

### Procedimiento en Supabase (producción)

Usar conexión **directa** (puerto **5432**), no el pooler (6543).

```powershell
cd MiNegocioCR.Api

# 1. Aplicar migraciones EF (recomendado antes del deploy)
$env:POSTGRES_CONNECTION_STRING = "<connection_string_supabase_5432>"
dotnet ef database update

# 2. Verificar schema
psql "<POSTGRES_CONNECTION_STRING>" -f Scripts/verify-schema.sql

# 3. Respaldo idempotente (si la BD quedó a medias en deploys anteriores)
psql "<POSTGRES_CONNECTION_STRING>" -f Scripts/apply-pending-migrations.sql
```

**Nota:** el paso 3 es seguro de ejecutar varias veces (`IF NOT EXISTS`). Úsalo si `verify-schema.sql` muestra columnas faltantes aunque `database update` diga "Done".

**SQL manual mínimo** (si solo falta quitar descuento legacy de reparaciones):

```sql
ALTER TABLE "RepairOrders" DROP COLUMN IF EXISTS "DiscountPercent";
```

Registrar en `__EFMigrationsHistory` la migración `20260526120000_RemoveRepairOrderDiscountPercent` si aplica (ver script completo).

La API también intenta migrar al arrancar en Railway; conviene aplicar el schema **antes** del redeploy para evitar el primer request fallando con 500.

### Datos mínimos

Debe existir al menos:

- Un registro en `Businesses`
- Configuración en `BusinessSettings`
- Un usuario en `Users`

Si la BD de producción ya tiene el tenant JoyCaTech u otro negocio, no hace falta re-seedear. Ver datos de ejemplo en `SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md` (solo referencia local).

---

## 7. Paso 4 — Deploy API (Railway)

### Configuración típica

| Setting | Valor |
|---------|-------|
| Root directory | Raíz del repo API (donde está `MiNegocioCR.Api.csproj`) |
| Runtime | .NET 8 (Railway auto-detecta) |
| Start | `dotnet MiNegocioCR.Api.dll` (Railway suele inferirlo) |

### URL de producción actual (frontend)

El build de producción del frontend apunta a:

```
https://minegociocrapi-production-bd7b.up.railway.app
```

Definido en `mi-negociocr-frontend/src/environments/environment.prod.ts`.

Si cambias la URL de Railway, actualiza ese archivo (o usa `public/env.js` — ver sección frontend).

### CORS

Orígenes permitidos (hardcoded en `Program.cs`):

- `http://localhost:4200`
- `https://localhost:7176`
- `https://mi-negociocr-frontend.vercel.app`
- `https://mi-negociocr.com`
- `https://www.mi-negociocr.com`

Si el dominio de Vercel es otro, hay que agregarlo en `Program.cs` o usar uno de los dominios listados.

### Health check

El endpoint raíz `/` responde OK, pero **`/health` no está implementado** aún. No configures health check en Railway apuntando a `/health` hasta que se agregue.

---

## 8. Paso 5 — Deploy frontend (Vercel)

### Build

```bash
cd mi-negociocr-frontend
npm run build
```

Salida: `dist/mi-negociocr-frontend/`

### URL del API

Por defecto el build usa `environment.prod.ts`:

```typescript
apiBaseUrl: 'https://minegociocrapi-production-bd7b.up.railway.app/api',
chatHubUrl: 'https://minegociocrapi-production-bd7b.up.railway.app/chatHub',
```

### Override en runtime (opcional)

`public/env.js` permite cambiar URLs sin rebuild:

```javascript
window.__env = {
  apiBaseUrl: 'https://tu-api.railway.app/api',
  chatHubUrl: 'https://tu-api.railway.app/chatHub'
};
```

### SPA routing (recomendado)

Agregar `vercel.json` en la raíz del frontend para evitar 404 al refrescar rutas como `/repairs` o `/reset-password`:

```json
{
  "rewrites": [{ "source": "/(.*)", "destination": "/index.html" }]
}
```

### Deploy en Vercel

- Conectar repo `mi-negociocr-frontend`
- Framework: Angular
- Build command: `npm run build`
- Output directory: `dist/mi-negociocr-frontend/browser` (verificar en `angular.json` según versión de Angular)

---

## 9. Paso 6 — Smoke test post-deploy

Checklist manual (5–10 minutos):

- [ ] **Login** con usuario de producción
- [ ] **Dashboard** carga sin errores 500 en consola del navegador
- [ ] **Ventas desde reparación — cortesía/donación:** servicio con descuento ₡ (modo fijo, no %) + abonos previos → saldo ₡0 → POST `/api/sales` sin 500; factura con desglose fiscal
- [ ] **Ventas desde reparación** con descuento (₡ o %) y abonos previos — POST `/api/sales` sin 500
- [ ] **Factura impresa** muestra línea de descuento cuando aplica
- [ ] **Dashboard** columna descuento en listado de ventas
- [ ] **Reparaciones — lista:** filtro por defecto **Pendiente**; buscar al escribir (sin botón)
- [ ] **Reparaciones — crear:** botón **+ Crear orden** abre modal; tipo **Impresora**; sin campo S.O. en formulario
- [ ] **Orden de reparación:** crear con **contacto ya existente** (no debe dar `PK_Contacts`)
- [ ] **Cambiar estado** de orden (ej. Pendiente → En progreso)
- [ ] **Vista previa** muestra ítems y totales
- [ ] **Enviar orden por correo** — debe llegar **una página** bien formada (HTML email, no iframe)
- [ ] **Venta POS** con cliente existente por teléfono + descuento — sin 500
- [ ] **Factura** con email del cliente → **Enviar por correo** funciona
- [ ] **Reset password** — correo con enlace a `App__PublicUrl`
- [ ] **Subir logo** del negocio (Supabase storage)
- [ ] **Menú lateral** — entrada **WhatsApp** oculta (módulo en pausa)
- [ ] **WhatsApp panel** — panel flotante recogido (💬); expandir/cerrar con botones
- [ ] **WhatsApp OAuth** — solo si está configurado (redirect + webhook)
- [ ] **Campaña CRM** — Clientes → Campaña: preview, encolar (asunto/cuerpo válidos), status avanza ~1 correo/min, **Detener campaña** detiene envíos; sin decenas de duplicados al mismo correo
- [ ] **Inventario — grid:** `/inventory` muestra tarjetas en varias columnas (desktop)
- [ ] **Inventario — filtros:** Productos / Servicios / Inactivos no se mezclan mal
- [ ] **Inventario — crear producto simple** (sin presentaciones) guarda correctamente
- [ ] **Inventario — detalle:** Ver → foto sin recorte feo; editar margen persiste
- [ ] **Inventario — presentación:** desactivar desde detalle multi-SKU → sin 500 (`PATCH .../toggle`)
- [ ] **Inventario — Editar** en tarjeta solo con 1 presentación

### Endpoints útiles para diagnóstico

```bash
# API viva
curl https://minegociocrapi-production-bd7b.up.railway.app/

# Login (ajustar credenciales)
curl -X POST https://minegociocrapi-production-bd7b.up.railway.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"...","password":"..."}'
```

---

## 10. Riesgos conocidos

### Críticos (resolver antes o inmediatamente después del deploy)

| Riesgo | Impacto | Acción |
|--------|---------|--------|
| Schema incompleto | 500 en dashboard y ventas | Ejecutar migraciones + `apply-pending-migrations.sql` |
| Variables faltantes | API no arranca o falla en runtime | Verificar tabla de variables obligatorias |
| `App__PublicUrl` incorrecta | Links de reset password rotos | Apuntar al dominio real del frontend |
| Código sin push | Prod sigue con bugs viejos | Commit + push en API y frontend |
| Contacto repetido sin fix | 500 `PK_Contacts` al crear orden o facturar | Deploy API con `RepairOrderContactHelper` + `RegisterSaleUseCase` actualizados |
| Factura reparación saldo ₡0 | 500 `TotalAmount` NULL en PostgreSQL | API ≥ `d15cd40`; ver FIXES_MAYO_2026 §25 |

### Importantes (estabilidad)

| Riesgo | Impacto | Acción futura |
|--------|---------|---------------|
| Swagger siempre expuesto | Superficie de ataque | Desactivar en `Production` |
| Sin `/health` | Monitoreo difícil en Railway | Implementar endpoint |
| DataProtection en disco local | Tokens WhatsApp inválidos tras restart en Railway | Persistir claves en storage compartido |
| `Meta:RedirectUri` en localhost | OAuth WhatsApp roto | Override en Railway |
| Secretos en `appsettings.Development.json` | Exposición si el repo es público | Rotar Resend/JWT y no commitear keys |

---

## 11. Orden recomendado (resumen)

```
1. dotnet test (API) + npm run build (frontend)
2. Commit + push → repo API (master)
3. Commit + push → repo frontend (main)
4. Verificar variables en Railway
5. dotnet ef database update (contra Supabase prod, puerto 5432)
6. psql ... -f Scripts/verify-schema.sql
7. psql ... -f Scripts/apply-pending-migrations.sql  (solo si verify falla)
8. Redeploy API en Railway
9. curl GET / → 200
10. Deploy frontend en Vercel
11. Smoke test completo (login, dashboard, contacto repetido, ventas con descuento, órdenes, correo, WhatsApp recogido)
```

---

## 12. Documentación del proyecto

Los `.md` de registro viven en la carpeta workspace **`Mi-negociocr/`** (monorepo local con API + frontend). Copias en `MiNegocioCR.Api/docs/` para respaldo en Git.

| Documento | Contenido |
|-----------|-----------|
| [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) | Changelog detallado de fixes (contactos, UI, ventas) |
| [REFACTOR_REPAIR_PAYMENTS_SALES.md](./REFACTOR_REPAIR_PAYMENTS_SALES.md) | Modelo financiero reparaciones/ventas |
| [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) | Setup local PostgreSQL, Resend, JoyCaTech |

---

## 13. Referencias

| Documento | Contenido |
|-----------|-----------|
| [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) | Changelog vivo de correcciones mayo 2026 |
| [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) | Setup local, Resend, dashboard 500, tenant JoyCaTech |
| [MiNegocioCR.Api/RESEND_SETUP.md](./MiNegocioCR.Api/RESEND_SETUP.md) | Configuración de correo Resend |
| [MiNegocioCR.Api/docs/local-dev-database.md](./MiNegocioCR.Api/docs/local-dev-database.md) | BD local y migraciones |
| [REFACTOR_REPAIR_PAYMENTS_SALES.md](./REFACTOR_REPAIR_PAYMENTS_SALES.md) | Modelo financiero reparaciones/ventas |
| `MiNegocioCR.Api/Scripts/apply-pending-migrations.sql` | Script schema pendiente (prod y local) |
| `mi-negociocr-frontend/src/environments/environment.prod.ts` | URLs API en build producción |

---

*Última actualización: 4 junio 2026 — migración Pedidos Internet; spec Créditos en backlog*
