# MiNegocioCR — API

Backend multi-tenant para **MiNegocioCR**: inventario, órdenes de reparación, ventas/POS, pedidos internet, créditos, dashboard, CRM de clientes, campañas de correo, WhatsApp e integraciones (Resend, Supabase Storage). **Próximo módulo:** tienda digital / marketplace — [MARKETPLACE_INICIO_JUNIO_2026.md](./docs/MARKETPLACE_INICIO_JUNIO_2026.md).

| | |
|---|---|
| **Stack** | .NET 8, ASP.NET Core, EF Core 8, PostgreSQL (Npgsql), JWT, SignalR, Resend |
| **Repositorio** | [github.com/azariasmurillo/MiNegocioCR.Api](https://github.com/azariasmurillo/MiNegocioCR.Api) |
| **Rama principal** | `master` |
| **Deploy** | [Railway](https://railway.app) — ver [docs/DEPLOY.md](./docs/DEPLOY.md) |
| **Frontend** | Repo separado `mi-negociocr-frontend` (Angular, Vercel) |

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 15+ (local: base `MiNegocioCR_Dev`)
- Opcional: `psql` para scripts en `Scripts/`

---

## Inicio rápido (local)

```powershell
cd MiNegocioCR.Api

# Migraciones
dotnet ef database update

# Ejecutar (perfil https → https://localhost:7176, Swagger en /swagger)
dotnet run --launch-profile https

# Tests
dotnet test
```

Configuración de desarrollo: `appsettings.Development.json` y variables en `Properties/launchSettings.json` (`POSTGRES_CONNECTION_STRING`).

Correos en local: [RESEND_SETUP.md](./RESEND_SETUP.md).

---

## Estructura del proyecto

```
MiNegocioCR.Api/
├── API/Controllers/          # Endpoints REST (/api/...)
├── Application/              # Casos de uso, validadores, cola de campañas
├── Domain/                   # Entidades y reglas de dominio
├── Infrastructure/           # EF Core, email, storage, background services
├── Migrations/               # Migraciones EF Core
├── MiNegocioCR.Tests/        # Tests unitarios e integración (PostgreSQL opcional)
├── Scripts/                  # SQL idempotente y emergencia
└── docs/                     # Documentación incluida en este repo
```

### Módulos principales

| Área | Controladores / servicios |
|------|-------------------------|
| Auth | `AuthController` — login, reset password |
| Negocio | `BusinessesController`, configuración |
| Inventario | `InventoryController`, `VariantController`, catálogo |
| Reparaciones | `RepairOrdersController`, pagos |
| Ventas | `SalesController` — POS, factura desde reparación |
| Dashboard | `DashboardController` — KPIs, tendencias, canales |
| CRM / campañas | `ContactsController` — clientes, cola de correo, cancelación |
| Pedidos Internet | `InternetOrdersController` — proxy Amazon, estados, correos |
| Créditos | `CreditAccountsController` — cuentas por cobrar |
| **Tienda digital** *(M1)* | Ver [MARKETPLACE_INICIO_JUNIO_2026.md](./docs/MARKETPLACE_INICIO_JUNIO_2026.md) |
| WhatsApp | `WhatsappController`, webhooks Meta |
| Admin | `AdminController` — panel `/admin` |

La cola de campañas corre en background (`CampaignQueueBackgroundService`): **495** correos/día (plataforma), **60 s** entre envíos. Detalle: [docs/email-campaigns-crm.md](./docs/email-campaigns-crm.md).

---

## Variables de entorno (producción)

Resumen; tabla completa en [docs/DEPLOY.md](./docs/DEPLOY.md).

| Variable | Uso |
|----------|-----|
| `POSTGRES_CONNECTION_STRING` | PostgreSQL (Supabase, puerto 5432 directo) |
| `Jwt__Key` | Firma JWT |
| `Admin__Password` | Panel admin (obligatorio en prod) |
| `Resend__ApiKey` / `Resend__FromEmail` | Envío de correos |
| `App__PublicUrl` | URL del frontend (links reset password) |
| `SUPABASE_URL` + `SUPABASE_SERVICE_ROLE_KEY` | Storage (logos, imágenes campaña) |

`APPLY_MIGRATIONS_ON_STARTUP=true` (default) aplica migraciones al arrancar en Railway.

---

## Base de datos

```powershell
dotnet ef database update
psql -U postgres -d MiNegocioCR_Dev -f Scripts/verify-schema.sql
```

Si EF falla: `Scripts/apply-pending-migrations.sql` (idempotente).

Guía local: [docs/local-dev-database.md](./docs/local-dev-database.md).

Migraciones críticas recientes: ventas/descuentos (`20260526*`), CRM y cola de campañas (`20260527*` … `20260529120000_AddEmailCampaignQueue`).

---

## Tests

```powershell
dotnet test
dotnet test --filter ContactCampaign
dotnet test --filter RegisterSalePostgresIntegrationTests   # requiere PostgreSQL local
```

Esperado: **160+** tests (integración Postgres se omite si no hay BD).

---

## Documentación

| Documento | Contenido |
|-----------|-----------|
| [docs/DEPLOY.md](./docs/DEPLOY.md) | Railway, variables, migraciones, smoke test |
| [docs/FIXES_MAYO_2026.md](./docs/FIXES_MAYO_2026.md) | Changelog de correcciones |
| [docs/email-campaigns-crm.md](./docs/email-campaigns-crm.md) | Campañas CRM (cola, límites, emergencia) |
| [docs/TIENDA_DIGITAL_DISENO_UNIFICADO.md](./docs/TIENDA_DIGITAL_DISENO_UNIFICADO.md) | Tienda digital — spec oficial |
| [docs/MARKETPLACE_LITE_DISENO_v1.md](./docs/MARKETPLACE_LITE_DISENO_v1.md) | Marketplace Lite — referencia Chat |
| [docs/local-dev-database.md](./docs/local-dev-database.md) | PostgreSQL local y verify-schema |
| [docs/PEDIDOS_INTERNET_DISENO_v1.md](./docs/PEDIDOS_INTERNET_DISENO_v1.md) | Pedidos Internet |
| [docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md) | Créditos / cuentas por cobrar (implementado) |
| [docs/CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md](./docs/CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md) | Layout responsive — menú colapsable (frontend) |
| [docs/INVENTARIO_UX_REDISENO_v2.md](./docs/INVENTARIO_UX_REDISENO_v2.md) | **Inventario UX** — diseño aprobado + plan de sprints |
| [docs/Inventory-API-Handoff.md](./docs/Inventory-API-Handoff.md) | APIs inventario/catálogo |
| [docs/SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./docs/SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) | Setup local y notas operativas |
| [docs/WORKSPACE_INDEX.md](./docs/WORKSPACE_INDEX.md) | Índice documentación monorepo |
| [docs/README.md](./docs/README.md) | Índice de `docs/` |
| [RESEND_SETUP.md](./RESEND_SETUP.md) | Configuración Resend |

`SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md` y el índice del workspace están en `docs/`. Al cambiar `docs/DEPLOY.md` o `docs/FIXES_MAYO_2026.md`, sincronizar con la raíz del monorepo `Mi-negociocr/` si aplica.

---

## Deploy

```powershell
dotnet test
git push origin master
```

Railway redeploy + `dotnet ef database update` contra Supabase (puerto **5432**) antes o justo después del release. Checklist: [docs/DEPLOY.md](./docs/DEPLOY.md).

---

*MiNegocioCR — API · .NET 8 · PostgreSQL*
