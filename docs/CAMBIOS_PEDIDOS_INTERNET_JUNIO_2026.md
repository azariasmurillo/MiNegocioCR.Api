# Pedidos Internet — code review y documentación de commit (Junio 2026)

**Workspace:** `c:\Mi-negociocr`  
**Repos Git separados:** `MiNegocioCR.Api` (`master`) · `mi-negociocr-frontend` (`main`)  
**Spec de producto:** [MiNegocioCR.Api/docs/PEDIDOS_INTERNET_DISENO_v1.md](./MiNegocioCR.Api/docs/PEDIDOS_INTERNET_DISENO_v1.md)  
**Próximo módulo (backlog):** [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./MiNegocioCR.Api/docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md)

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Code review — bloqueantes y recomendaciones](#2-code-review--bloqueantes-y-recomendaciones)
3. [Qué incluye el release](#3-qué-incluye-el-release)
4. [Archivos por repositorio](#4-archivos-por-repositorio)
5. [Pre-commit checklist](#5-pre-commit-checklist)
6. [Deploy y base de datos](#6-deploy-y-base-de-datos)
7. [Plan de pruebas manual](#7-plan-de-pruebas-manual)
8. [Mensajes de commit sugeridos](#8-mensajes-de-commit-sugeridos)

---

## 1. Resumen ejecutivo

| Área | Entregable | Estado |
|------|------------|--------|
| API | Módulo `internet-orders`: CRUD, estados, correos automáticos, envío manual HTML | ✅ Listo para commit |
| BD | Migración `20260604142659_AddInternetOrders` + script idempotente | ✅ (fix `Down()` aplicado) |
| Frontend | Ruta `/internet-orders`, crear/listar/gestionar, ojito vista previa/correo | ✅ Build OK |
| Correo cliente | HTML con líneas USD + totales ₡, **sin tipo de cambio**, sin botón «Ver detalle» | ✅ |
| UX | Panel resumen CRC (formato ₡ corregido, componente compartido) | ✅ |
| Tests API | ~9 tests unitarios/integración in-memory (`InternetOrder*`) | ✅ (correr con API detenida) |

---

## 2. Code review — bloqueantes y recomendaciones

### Bloqueantes antes de producción (corregidos en esta sesión de review)

| # | Hallazgo | Acción |
|---|----------|--------|
| B1 | Migración `Down()` eliminaba `EmailCampaignRecipients` sin haberla creado en `Up()` | **Corregido** en `Migrations/20260604142659_AddInternetOrders.cs` |
| B2 | `DataProtection-Keys/*.xml` aparecía como untracked | **Añadido** a `.gitignore` — no commitear |

### Riesgos conocidos (mismo patrón que el resto del API — documentar, no bloquean este PR)

| # | Riesgo | Severidad | Notas |
|---|--------|-----------|-------|
| R1 | `businessId` en URL no se valida contra JWT (`business_id`) | Alta (IDOR teórico) | Igual que reparaciones/ventas; endurecer en PR futuro con `AuthHelper` |
| R2 | `POST .../send-email` acepta HTML del cliente y email opcional | Media | Mismo modelo que reparaciones; solo staff autenticado |
| R3 | Número diario de pedido: posible colisión concurrente | Media | Índice único `(BusinessId, OrderNumber)` — segundo intento falla |
| R4 | `UpdateInternetOrder` sin transacción explícita | Media | Reemplazo líneas/adelantos; fallo parcial poco probable |
| R5 | Edición permitida en API aunque UI bloquee pedidos terminales | Baja | Coherente con spec v1 |

### Frontend — mejoras opcionales (post-merge)

| # | Tema | Archivo |
|---|------|---------|
| F1 | Permitir submit con `totalsError` activo (API rechaza, UX confusa) | `internet-orders.ts`, `internet-order-edit-dialog.ts` |
| F2 | `valueChanges` sin `takeUntilDestroyed` en diálogo | `internet-order-edit-dialog.ts` |
| F3 | `orders` vs `visibleOrders` duplicados | `internet-orders.ts` |
| F4 | Etiqueta columna «Total ₡» vs valor `subtotalCrc` | `internet-orders.html` |

### Verificación automática

```powershell
# API detenida (Visual Studio / dotnet run) para evitar lock de MiNegocioCR.Api.exe
cd c:\Mi-negociocr\MiNegocioCR.Api
dotnet test

cd c:\Mi-negociocr\mi-negociocr-frontend
npm run build
```

---

## 3. Qué incluye el release

### Backend

- **Entidades:** `InternetOrder`, `InternetOrderLine`, `InternetOrderAdvance`, enum `InternetOrderStatus`.
- **Reglas:** calculadora CRC/USD, FSM de estados, numeración diaria `YYYYMMDD###`.
- **Endpoints** (`/api/internet-orders`):
  - `GET business/{businessId}` — listado con filtro estado/búsqueda
  - `GET|POST|PUT {businessId}` / `{businessId}/{id}`
  - `PATCH {businessId}/{id}/status`
  - `POST {businessId}/{id}/send-email` — HTML generado en frontend
- **Correos automáticos:** al pasar a **Comprada** y **Recibida** (`InternetOrderNotificationService` + `InternetOrderEmailHtmlBuilder`).
- **Correo manual:** mismo HTML que la vista previa (sin botón CTA).

### Frontend

- Menú **Pedidos Internet** en sidebar.
- Lista con búsqueda, **ojito** → modal vista previa (Imprimir / Enviar correo / Cerrar).
- Formulario **Crear pedido** (líneas USD, costos internos, adelantos, resumen CRC).
- Diálogo **Gestionar pedido** (estado, edición, tracking opcional).
- Ruta impresión embebida: `/internet-orders/:id/print?embedded=1`.
- `proxy.conf.json` → API local `127.0.0.1:5273` (reiniciar `ng serve` si cambió).

### Reglas de negocio (recordatorio)

- Tipo de cambio: **solo staff** en API detalle; **nunca** en correo al cliente.
- Cada línea requiere URL `https://...`.
- Estados: `Created → Purchased → Received → Delivered`; cancelación desde no terminales.

---

## 4. Archivos por repositorio

### `MiNegocioCR.Api` — commitear

| Grupo | Rutas |
|-------|--------|
| API | `API/Controllers/InternetOrdersController.cs`, `API/Program.cs` |
| Dominio | `Domain/Entities/InternetOrder*.cs`, `Domain/Enums/InternetOrderStatus.cs`, `Domain/Entities/Contact.cs` |
| Aplicación | `Application/Common/InternetOrder*.cs`, `Application/DTOs/InternetOrderDtos.cs`, `Application/Interfaces/InternetOrders/*`, `Application/UseCases/InternetOrders/*`, `Application/Interfaces/IInternetOrderNotificationService.cs` |
| Infra | `Infrastructure/Services/InternetOrderNotificationService.cs`, `Infrastructure/Persistence/AppDbContext.cs`, `Application/Interfaces/IAppDbContext.cs` |
| Migración | `Migrations/20260604142659_AddInternetOrders.cs` (+ `.Designer.cs`), `Migrations/AppDbContextModelSnapshot.cs` |
| Tests | `MiNegocioCR.Tests/Application/Common/InternetOrder*.cs`, `MiNegocioCR.Tests/UseCases/InternetOrders/*` |
| Scripts/docs | `Scripts/apply-pending-migrations.sql`, `docs/PEDIDOS_INTERNET_DISENO_v1.md`, `docs/README.md`, `docs/local-dev-database.md` |

### `MiNegocioCR.Api` — NO commitear

- `DataProtection-Keys/`
- `bin/`, `obj/`, `.vs/`
- Secretos locales en `appsettings.Development.json` (si contienen keys reales)

### `mi-negociocr-frontend` — commitear

| Grupo | Rutas |
|-------|--------|
| Feature | `src/app/features/internet-orders/**` |
| Integración | `src/app/app.routes.ts`, `src/app/layout/sidebar/sidebar.html` |
| Compartido | `src/app/features/shared/components/print-preview-dialog/print-preview-dialog.ts` |
| Proxy | `proxy.conf.json` (solo si el equipo acuerda puerto 5273 para todos) |

---

## 5. Pre-commit checklist

### API (`MiNegocioCR.Api`)

- [ ] Detener proceso API que bloquea `MiNegocioCR.Api.exe`
- [ ] `dotnet test` — verde (160+ tests históricos + nuevos InternetOrder)
- [ ] `git status` — sin `DataProtection-Keys/`
- [ ] Revisar que `apply-pending-migrations.sql` incluye bloque Internet Orders
- [ ] Commit solo archivos de la tabla §4

### Frontend (`mi-negociocr-frontend`)

- [ ] `npm run build` — sin errores TypeScript
- [ ] `git add src/app/features/internet-orders/` + archivos de integración
- [ ] Probar flujo manual §7 en local

### Workspace (documentación)

- [ ] Este archivo + entrada en `FIXES_MAYO_2026.md` §29
- [ ] Copia en `MiNegocioCR.Api/docs/CAMBIOS_PEDIDOS_INTERNET_JUNIO_2026.md` (mismo contenido)

---

## 6. Deploy y base de datos

### Local

```powershell
cd MiNegocioCR.Api
dotnet ef database update
# opcional respaldo:
psql -U postgres -d MiNegocioCR_Dev -f Scripts/apply-pending-migrations.sql
```

### Producción (Railway / Supabase)

1. Deploy API (aplica migraciones al arranque si `APPLY_MIGRATIONS_ON_STARTUP=true`).
2. Si la BD quedó a medias, ejecutar `Scripts/apply-pending-migrations.sql` en Supabase.
3. Verificar tablas:

```sql
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('InternetOrders', 'InternetOrderLines', 'InternetOrderAdvances');
```

4. Deploy frontend Vercel (`main`).

Detalle BD: [MiNegocioCR.Api/docs/local-dev-database.md](./MiNegocioCR.Api/docs/local-dev-database.md) — sección *Pedidos Internet*.

---

## 7. Plan de pruebas manual

| # | Caso | Esperado |
|---|------|----------|
| 1 | Crear pedido con 2 líneas USD + adelanto | Lista muestra totales; resumen CRC sin ₡ duplicado |
| 2 | Clic fila → Gestionar | Diálogo carga; cambiar estado válido guarda |
| 3 | Ojito → vista previa | Modal con diseño de correo; Imprimir OK |
| 4 | Enviar por correo (cliente con email) | Snackbar éxito; correo sin botón «Ver detalle» |
| 5 | Cambiar a Comprada / Recibida | Correo automático si `EnableEmailNotifications` |
| 6 | Línea sin `https://` | API rechaza con mensaje claro |
| 7 | Adelantos > total | Panel error; idealmente bloquear submit (mejora F1) |
| 8 | `?orderId=` en URL | Abre gestionar (legacy enlaces viejos) |

---

## 8. Mensajes de commit sugeridos

### API (`master`)

```
feat(api): add Internet Orders module with status workflow and email notifications

Introduce InternetOrders CRUD, daily order numbers, CRC/USD totals, status
transitions with auto customer emails on Purchased/Received, manual send-email
endpoint, EF migration for orders/lines/advances, and unit tests.
```

### Frontend (`main`)

```
feat(internet-orders): add assisted purchase orders module

Add list/create UI, manage dialog, CRC totals panel, print preview and email
integration, routes and sidebar. Extend print-preview for internet-order docs.
```

---

## Referencias

- [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) — §29 Pedidos Internet, §30 Créditos (backlog)
- [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./MiNegocioCR.Api/docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md) — spec cuentas por cobrar
- [DEPLOY.md](./DEPLOY.md) — flujo Railway/Vercel
- [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) — PostgreSQL y tenant local

---

## 9. UX Junio 2026 (solo frontend)

Release posterior al módulo inicial: modal crear, búsqueda automática, filtro **Creada** por defecto, espaciado en diálogo **Gestionar pedido**.

**Documentación:** [CAMBIOS_PEDIDOS_INTERNET_UX_JUNIO_2026.md](./CAMBIOS_PEDIDOS_INTERNET_UX_JUNIO_2026.md) · FE: `mi-negociocr-frontend/docs/CAMBIOS_PEDIDOS_INTERNET_UX_JUNIO_2026.md`

Sin migraciones ni cambios de API.

---

*Última actualización: 11 junio 2026 — UX Pedidos Internet (modal + búsqueda auto)*
