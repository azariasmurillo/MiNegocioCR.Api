# Registro de fixes — Mayo 2026

Historial de correcciones aplicadas en MiNegocioCR (API + frontend).  
**Se actualiza en cada fix** hasta que indiques *“seguimos con otro fix”* — entonces pasamos al siguiente cambio en código y lo documentamos aquí.

**Relacionado:**

- [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) — setup local, reset password, dashboard 500, UI reparaciones
- [DEPLOY.md](./DEPLOY.md) — guía de deploy a producción
- [REFACTOR_REPAIR_PAYMENTS_SALES.md](./REFACTOR_REPAIR_PAYMENTS_SALES.md) — refactor financiero reparaciones/ventas

---

## Índice

1. [Resumen rápido](#1-resumen-rápido)
2. [Backend — estados de orden y ventas](#2-backend--estados-de-orden-y-ventas)
3. [Backend — ítems en detalle de orden](#3-backend--ítems-en-detalle-de-orden)
4. [Backend — schema BD (migraciones huérfanas)](#4-backend--schema-bd-migraciones-huérfanas)
5. [Frontend — ítems en vista previa de orden](#5-frontend--ítems-en-vista-previa-de-orden)
6. [Frontend — modal vista previa (UX)](#6-frontend--modal-vista-previa-ux)
7. [Frontend — correo de orden (HTML dedicado)](#7-frontend--correo-de-orden-html-dedicado)
8. [Frontend — impresión en una sola hoja](#8-frontend--impresión-en-una-sola-hoja)
9. [Frontend — bucle infinito ResizeObserver](#9-frontend--bucle-infinito-resizeobserver)
10. [Frontend — variante perdida al registrar pago](#10-frontend--variante-perdida-al-registrar-pago)
11. [Frontend — dropdown sistema operativo](#11-frontend--dropdown-sistema-operativo)
12. [Ventas — Completar restante en pagos](#12-ventas--completar-restante-en-pagos)
13. [Fixes previos (referencia)](#13-fixes-previos-referencia)
14. [Documentación generada en sesión](#14-documentación-generada-en-sesión)
15. [Pendiente / deploy](#15-pendiente--deploy)
16. [Ventas — Pagos abajo en carrito](#16-ventas--pagos-abajo-en-carrito)
17. [Ventas — Efectivo precargado y botones activos](#17-ventas--efectivo-precargado-y-botones-activos)
18. [Reparaciones — contacto repetido al crear orden](#18-reparaciones--contacto-repetido-al-crear-orden)
19. [Ventas — contacto repetido y email en factura](#19-ventas--contacto-repetido-y-email-en-factura)
20. [UI — WhatsApp panel recogido por defecto](#20-ui--whatsapp-panel-recogido-por-defecto)
21. [Dashboard — datos desde Sales + zona Costa Rica](#21-dashboard--datos-desde-sales--zona-costa-rica)

---

## 1. Resumen rápido

| # | Área | Problema | Estado |
|---|------|----------|--------|
| 1 | API | Cambiar estado de orden no persistía | ✅ |
| 2 | API | GET orden sin ítems enriquecidos del catálogo | ✅ |
| 3 | BD | Dashboard/ventas 500 por schema incompleto | ✅ (script) |
| 4 | UI | Vista previa sin ítems de inventario | ✅ |
| 5 | UI | Modal cortado / botones duplicados | ✅ |
| 6 | Email | Correo de orden en 3 páginas mal formadas | ✅ |
| 7 | Print | Impresión de orden en 3 hojas | ✅ |
| 8 | UI | Consola con miles de errores ResizeObserver | ✅ |
| 9 | UI | Variante perdida al registrar pago sin guardar orden | ✅ |
| 10 | UI | S.O. en crear orden — dropdown (Windows default) | ✅ |
| 11 | Ventas | Completar restante en pagos (orden reparada) | ✅ |
| 12 | Ventas | Métodos de pago abajo del carrito (cerca de Facturar) | ✅ |
| 13 | Ventas | Efectivo precargado + botones activos con ítems | ✅ |
| 14 | Reparaciones | Error `PK_Contacts` al crear orden con cliente existente | ✅ |
| 15 | Ventas | Error `PK_Contacts` al facturar + email vacío en factura | ✅ |
| 16 | UI | WhatsApp panel expandido al cargar (molesto) | ✅ |
| 17 | Dashboard | Reparaciones = 0, fechas UTC, totales mal interpretados | ✅ |

---

## 2. Backend — estados de orden y ventas

### Síntoma

Al cambiar el estado de una orden de reparación (Recibida, En reparación, Entregada, etc.), la UI parecía actualizar pero el cambio **no se guardaba** en base de datos.

### Causa

`AppDbContext` usa `QueryTrackingBehavior.NoTracking` global en `Program.cs`. Los use cases de actualización cargaban entidades sin `.AsTracking()`, así que `SaveChanges()` no persistía cambios.

### Fix

Agregar `.AsTracking()` al cargar la entidad antes de modificarla:

| Archivo | Cambio |
|---------|--------|
| `MiNegocioCR.Api/Application/UseCases/RepairOrder/UpdateRepairOrderStatusUseCase.cs` | `.AsTracking()` al obtener la orden |
| `MiNegocioCR.Api/Application/UseCases/RepairOrder/UpdateRepairOrderUseCase.cs` | `.AsTracking()` al obtener la orden |
| `MiNegocioCR.Api/Application/UseCases/Sales/RegisterSaleUseCase.cs` | `.AsTracking()` al marcar orden como Entregada al facturar |

### Tests

- `MiNegocioCR.Tests/UseCases/RepairOrder/UpdateRepairOrderStatusUseCaseTests.cs`
- Regresión documentada en tests de `GetRepairOrderByIdUseCase` con contexto `NoTracking`

---

## 3. Backend — ítems en detalle de orden

### Síntoma

El endpoint `GET /api/repair-orders/{id}` devolvía ítems con descripción genérica o incompleta; la vista previa no mostraba bien repuestos del catálogo.

### Causa

Faltaban `Include` de variante/catálogo y proyección enriquecida al mapear ítems.

### Fix

| Archivo | Cambio |
|---------|--------|
| `MiNegocioCR.Api/Application/UseCases/RepairOrder/GetRepairOrderByIdUseCase.cs` | `Include` de ítems → variante → catálogo; mapeo en memoria |
| `MiNegocioCR.Api/Application/Common/RepairOrderItemProjection.cs` | **Nuevo** — descripción enriquecida (nombre + SKU del catálogo) |
| `MiNegocioCR.Tests/UseCases/RepairOrder/GetRepairOrderByIdUseCaseTests.cs` | **Nuevo** — cobertura del detalle con ítems |

---

## 4. Backend — schema BD (migraciones huérfanas)

### Síntoma

Errores **500** en dashboard y ventas (`profit-by-source`, `sales-trend`, listado de ventas).

### Causa

Dos migraciones existían en código **sin `.Designer.cs`**, por lo que `dotnet ef database update` las ignoraba:

- `20260504220000_AddSaleCostAndProfitMetrics`
- `20260522120000_RefactorPaymentsAndSalePaymentMethods`

El código ya consultaba columnas/tablas que no existían en PostgreSQL.

### Fix

Script idempotente:

```
MiNegocioCR.Api/Scripts/apply-pending-migrations.sql
```

Ejecutar **después** de `dotnet ef database update` (local y producción).

Documentación: `MiNegocioCR.Api/docs/local-dev-database.md`

---

## 5. Frontend — ítems en vista previa de orden

### Síntoma

La vista previa / impresión de orden no mostraba ítems de inventario (ej. “Memoria”).

### Causa

Mapper del API no extraía bien el array `items`; etiquetas de variantes no resolvían nombre del catálogo.

### Fix

| Archivo | Cambio |
|---------|--------|
| `mi-negociocr-frontend/src/app/features/repairs/services/repair-order-api.mapper.ts` | Mejor extracción de `items` y unwrap de respuesta |
| `mi-negociocr-frontend/src/app/features/repairs/utils/repair-order-item-label.util.ts` | **Nuevo** — etiquetas con variantes del catálogo |
| `mi-negociocr-frontend/src/app/features/repairs/pages/repair-order-print/repair-order-print.ts` | Carga variantes; `itemLabel()` en tabla |
| `mi-negociocr-frontend/src/app/features/repairs/pages/repair-order-print/repair-order-print.html` | Tabla de ítems en cuerpo principal |

---

## 6. Frontend — modal vista previa (UX)

### Síntoma

- Contenido cortado en el modal
- Botones **Imprimir / Enviar** duplicados (toolbar del iframe + modal)

### Fix

| Archivo | Cambio |
|---------|--------|
| `print-preview-dialog.ts` | URL con `?embedded=1`; autoaltura iframe; botones en modal |
| `print-preview-dialog.scss` | Scroll en modal; acciones sticky |
| `repair-order-print.ts` | `embeddedMode` oculta toolbar interna |
| `repairs.ts`, `dashboard.ts`, `sales-manual.ts` | Diálogo con `maxHeight: '95vh'` |

---

## 7. Frontend — correo de orden (HTML dedicado)

### Síntoma

Al enviar orden por correo desde vista previa, llegaban **3 páginas largas mal formadas** (sidebar apilada, sin estilos de email).

### Causa

`PrintPreviewDialogComponent.sendEmail()` enviaba el **HTML completo del iframe** (Angular + CSS de la app). Los clientes de correo no lo interpretan como un navegador.

### Fix

Generar HTML de email con tablas e estilos inline (misma información que la vista previa):

| Archivo | Cambio |
|---------|--------|
| `repair-order-email-html.ts` | Reescrito — plantilla email compacta; `buildRepairOrderEmailHtmlPayload()` |
| `print-preview-dialog.ts` | `sendEmail()` usa payload dedicado (orden + balance + pagos + config + variantes), no iframe |
| Ventas en modal | `renderSaleEmailHtml()` con datos frescos de API |

**Nota:** en producción el envío real lo hace `ResendEmailService` en Railway (`Resend__ApiKey`).

---

## 8. Frontend — impresión en una sola hoja

### Síntoma

Al imprimir desde vista previa, el navegador generaba **3 hojas** para una orden típica.

### Causa

`repair-order-print.scss` tenía estilos `@media print` mínimos (sidebar alto, `min-height: 720px`, márgenes grandes, contenido duplicado).

### Fix

Estilos de impresión compactos (mismo enfoque que `sales-invoice-print.scss`):

| Cambio en print | Detalle |
|-----------------|---------|
| Grid compacto | Sidebar ~168px, `align-items: start` |
| Tipografías | Tamaños reducidos |
| Espaciado | Márgenes y padding mínimos |
| Ocultos en print | Contacto duplicado, franja rápida de totales |
| Problema | Sin `min-height` ni líneas de fondo |
| Página | `@page { margin: 6mm 8mm; }` |

| Archivo | Cambio |
|---------|--------|
| `repair-order-print.scss` | Bloque `@media print` ampliado |
| `angular.json` | Budget `anyComponentStyle` → 12 kB (plantillas de impresión) |

---

## 9. Frontend — bucle infinito ResizeObserver

### Síntoma

Al abrir vista previa, la consola se llenaba de miles de errores:

```
ResizeObserver loop completed with undelivered notifications
```

Parecía quedar “cargando” indefinidamente.

### Causa

Feedback loop: `ResizeObserver` en el iframe → `syncIframeHeight()` cambiaba altura del iframe → nuevo resize → observer otra vez. Además se observaban `body` y `documentElement`, y había polling de 10 s.

### Fix

| Archivo | Cambio |
|---------|--------|
| `print-preview-dialog.ts` | `scheduleIframeHeightSync()` con `requestAnimationFrame` |
| | Solo actualiza altura si cambió ≥ 2 px |
| | Observa solo `body` |
| | Desconecta observer tras ~3 s |
| | Limpieza al cerrar modal |
| `repair-order-print.ts` / `.html` | `onEmbeddedContentResize()` al cargar logo |

---

## 10. Frontend — variante perdida al registrar pago

### Síntoma

En **Gestionar orden**: se agrega una variante del inventario y luego se registra un pago (sin haber guardado la orden antes). Al guardar el pago, **desaparece la variante** de la tabla de ítems.

### Causa

Tras crear el pago, el diálogo hacía `getById` + `hydrateForm(merged)`, reconstruyendo el formulario desde el servidor. Los ítems agregados solo existían en el **FormArray local** (aún no persistidos), así que se sobrescribían.

### Fix

| Archivo | Cambio |
|---------|--------|
| `repair-order-edit-dialog.ts` | `applyPaymentRefreshOnly()` — actualiza pagos y balance en `this.order` **sin** `hydrateForm` |
| | `readItemsFromForm()` — el formulario sigue siendo la fuente de verdad de ítems mientras el modal está abierto |

### Verificación

1. Gestionar orden → agregar variante (no guardar orden).
2. Registrar pago → guardar.
3. La variante **sigue visible** en la tabla; el pago aparece en pagos registrados.
4. Guardar orden para persistir ítems + pagos en BD.

---

## 11. Frontend — dropdown sistema operativo

### Síntoma

En **crear orden**, el sistema operativo era un campo de texto libre.

### Fix

Dropdown con las 4 opciones más habituales; **Windows** seleccionado por defecto:

1. Windows  
2. Linux  
3. Mac  
4. Chrome OS  

| Archivo | Cambio |
|---------|--------|
| `repair-order-operating-system.options.ts` | **Nuevo** — catálogo + default |
| `repairs.html` / `repairs.ts` | `mat-select` en crear orden |
| `repair-order-edit-dialog.ts` | Mismo dropdown en gestionar orden (valores antiguos se conservan) |

### Verificación

Abrir crear orden → S.O. muestra **Windows** preseleccionado; se puede cambiar a Linux, Mac o Chrome OS.

---

## 12. Ventas — Completar restante en pagos

### Síntoma

Al facturar una orden reparada con pago mixto (ej. ₡5 000 SINPE + ₡15 000 efectivo), había que tipear cada monto a mano.

### Fix

En ventas desde reparación (`/sales?repairOrderId=...`):

- Indicador **Saldo a cobrar / Asignado / Falta o Sobrante**
- Botón **«Completar restante»** en cada fila de método de pago
- Fórmula: `monto fila = saldo pendiente − suma(otros métodos)`; si no hay otros montos, carga el saldo completo

| Archivo | Cambio |
|---------|--------|
| `sales-manual.ts` | `fillPaymentRemainder()`, getters de asignación |
| `sales-manual.html` | Resumen + botón por fila (solo `hasRepairOrderSummary`) |
| `sales-manual.scss` | Estilos del resumen de asignación |

**Ventas normales** (sin reparación): sin cambios.

### Verificación

1. Orden reparada → Facturar → agregar SINPE + Efectivo.
2. Escribir ₡5 000 en SINPE → **Completar restante** en Efectivo → ₡15 000 (si saldo era ₡20 000).
3. Solo Efectivo + **Completar restante** → saldo completo en efectivo.

---

## 16. Ventas — Pagos abajo en carrito

### Síntoma

En venta desde reparación el saldo pendiente aparecía al pie del carrito, pero los métodos de pago estaban en otra tarjeta **arriba**. El usuario tenía que subir para agregar cobros y el botón **Guardar y generar factura** quedaba bloqueado sin una pista clara de dónde actuar.

### Fix

- Se eliminó la tarjeta independiente **Métodos de pago**.
- La sección de cobro quedó **dentro del carrito**, justo **encima** de «Guardar venta» / «Guardar y generar factura».
- Si falta método o monto, el snackbar indica «iconos abajo, junto a Facturar» y hace scroll + resalte breve a `#checkout-payments`.
- En reparación con saldo pendiente, el resumen **Saldo a cobrar / Asignado / Falta** se muestra aunque aún no haya métodos agregados.

| Archivo | Cambio |
|---------|--------|
| `sales-manual.html` | `checkout-payments` dentro de `cart-card`, antes de `.actions` |
| `sales-manual.scss` | Estilos de zona de cobro al pie + animación de resalte |
| `sales-manual.ts` | `getSubmitBlockReason()`, `focusCheckoutPayments()` |

### Verificación

1. Orden reparada → Facturar: ver ítems, totales, métodos de pago y botones en un solo flujo vertical.
2. Sin agregar método → clic en **Guardar y generar factura** → mensaje claro + scroll al bloque de pagos.
3. Agregar Efectivo + **Completar restante** → facturar con éxito.

---

## 17. Ventas — Efectivo precargado y botones activos

### Síntoma

**Guardar venta** / **Guardar y generar factura** quedaban desactivados hasta agregar manualmente un método de pago y tipear el monto.

### Fix

- Al cargar ítems (POS o venta desde reparación), se precarga **Efectivo** con el **total a cobrar** (saldo pendiente en reparación).
- Si solo hay Efectivo, el monto se actualiza cuando cambian cantidades, precios o descuento.
- Pagos mixtos: el usuario puede agregar SINPE, tarjeta, etc.; no se pisan montos ya definidos.
- Los botones de guardar quedan **activos** con ítems en el carrito; cliente y pagos se validan al hacer clic.

| Archivo | Cambio |
|---------|--------|
| `sales-manual.ts` | `syncDefaultCashPayment()`, `canSubmit` relajado |
| `sales-manual.html` | `(ngModelChange)` en descuento para resincronizar efectivo |

### Verificación

1. Venta desde reparación → Efectivo ya muestra el saldo → **Guardar y generar factura** habilitado (con cliente cargado).
2. POS normal → agregar producto → Efectivo = total → facturar.
3. Agregar SINPE además de Efectivo → ajustar montos o usar **Completar restante**.

---

## 18. Reparaciones — contacto repetido al crear orden

### Síntoma

Al **crear una orden de reparación** para un cliente que ya existe (mismo contacto o mismo teléfono), la API respondía **500** con:

```
23505: duplicate key value violates unique constraint "PK_Contacts"
```

Un mismo cliente debe poder traer todas las máquinas que quiera; no debería fallar por reutilizar el contacto.

### Causa

Mismo patrón que en ventas (`RegisterSaleUseCase`):

1. `AppDbContext` usa `QueryTrackingBehavior.NoTracking` global en `Program.cs`.
2. `RepairOrderContactHelper` devolvía el contacto existente **detached** (sin tracking).
3. `CreateRepairOrderUseCase` asignaba la navegación `Contact = contact` además de `ContactId`.
4. Al guardar, EF interpretaba el contacto como entidad nueva e intentaba un `INSERT` con el mismo `Id` → violación de `PK_Contacts`.

### Fix

| Archivo | Cambio |
|---------|--------|
| `MiNegocioCR.Api/Application/Common/RepairOrderContactHelper.cs` | `.AsTracking()` al buscar contacto por `contactId` o por teléfono |
| `MiNegocioCR.Api/Application/UseCases/RepairOrder/CreateRepairOrderUseCase.cs` | Solo `ContactId = contact.Id`; **sin** navegación `Contact = contact` |
| `MiNegocioCR.Api/Application/UseCases/RepairOrder/UpdateRepairOrderUseCase.cs` | `.AsTracking()` al cargar contacto; solo actualizar `ContactId`, no reasignar `Contact` |
| `MiNegocioCR.Tests/Application/Common/RepairOrderContactHelperTests.cs` | **Nuevo** — regresión con contexto `NoTracking` |

**Regla:** al persistir ventas u órdenes, usar solo `ContactId`. La resolución de contacto debe ir con `.AsTracking()` cuando se reutiliza o actualiza un registro existente.

### Verificación

1. **Reiniciar la API** después del build (si `MiNegocioCR.Api.exe` quedó bloqueando el binario, el fix no aplica hasta reiniciar).
2. Crear orden de reparación eligiendo un contacto ya existente en el autocomplete → debe guardar sin error.
3. Crear otra orden con el mismo teléfono (sin elegir contacto) → debe reutilizar el mismo `ContactId`, no duplicar fila en `Contacts`.
4. Tests: `dotnet test --filter FullyQualifiedName~RepairOrderContactHelperTests`

---

## 19. Ventas — contacto repetido y email en factura

### Síntoma

1. Al **guardar una venta** (POS o desde reparación) con un cliente que ya existía por teléfono → **500** con `23505: duplicate key value violates unique constraint "PK_Contacts"`.
2. Tras facturar con email del cliente en el formulario, **Enviar por correo** decía que no había email y la factura impresa mostraba **—** en correo.

### Causa

1. Mismo bug de `NoTracking` global: `SaleContactResolution` devolvía contacto detached; `RegisterSaleUseCase` asignaba navegación `Contact` en la venta → EF intentaba `INSERT` duplicado.
2. `GetSaleById` no exponía `customerEmail` plano; el mapper del frontend no leía `Contact.email` anidado; ventas desde reparación ignoraban el email del formulario si ya había `ContactId`.

### Fix

| Archivo | Cambio |
|---------|--------|
| `Application/Common/SaleContactResolution.cs` | `.AsTracking()` al buscar contacto por teléfono |
| `Application/UseCases/Sales/RegisterSaleUseCase.cs` | Solo `ContactId` en `Sale`; `ApplyContactDetailsFromRequest()` para actualizar email/nombre |
| `Application/UseCases/Sales/GetSaleByIdUseCase.cs` | Expone `CustomerName`, `CustomerEmail` planos |
| `MiNegocioCR.Tests/.../RegisterSaleUseCaseTests.cs` | Regresión `WithGlobalNoTracking_ReusesExistingContactByPhone` |
| `sales.service.ts` | Siempre envía `customerName` / `customerEmail` aunque haya `contactId` |
| `sale-invoice-api.mapper.ts` | Lee email desde raíz o `Contact.email` |
| `sale-invoice-from-repair-draft.ts` | Preserva email del formulario al mergear borrador |

**Regla compartida con reparaciones (§18):** persistir solo FK `ContactId`; nunca asignar navegación `Contact` al crear entidades con contacto resuelto por helper.

### Verificación

1. Venta POS con teléfono de contacto existente + descuento → POST `/api/sales` sin 500.
2. Venta desde reparación con abonos previos + email en formulario → factura muestra correo y **Enviar por correo** funciona.
3. Tests: `dotnet test --filter FullyQualifiedName~RegisterSaleUseCaseTests`

---

## 20. UI — WhatsApp panel recogido por defecto

### Síntoma

El panel lateral de WhatsApp ocupaba mucho espacio al entrar a la app (390px expandido). Se quería que **apareciera recogido** y solo se estire al pulsar el botón de expandir (💬), y se cierre con **◀**.

### Fix

| Archivo | Cambio |
|---------|--------|
| `layout/whatsapp-panel/whatsapp-panel.ts` | `collapsed = true` por defecto |
| `layout/layout-shell/layout-shell.scss` | Eliminada regla `@media (max-width: 1360px)` que ocultaba el panel por completo |

El chat sigue cargando conversaciones al iniciar (`layout-shell.ts` → `loadConversations()`). SignalR arranca al montar el panel (aunque esté recogido).

### Verificación

1. Entrar al dashboard → solo se ve la tira/botón verde 💬 a la derecha.
2. Clic en 💬 → panel completo con lista de conversaciones.
3. Clic en ◀ → vuelve a estado recogido.

---

## 13. Fixes previos (referencia)

Documentados en [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md):

| Tema | Fix |
|------|-----|
| Reset password no guardaba | `.AsTracking()` en `PasswordResetTokenRepository.cs` |
| Dashboard 500 | Script `apply-pending-migrations.sql` |
| Sección facturación en editar orden | Removida de `repair-order-edit-dialog.ts` (alineado al refactor financiero) |
| Emails dev | Resend en `appsettings.Development.json` |
| `ConsoleEmailService` | Solo para dev local; no registrado en prod |
| Contacto repetido en ventas | Ver §19 — `SaleContactResolution` + `RegisterSaleUseCase` |

---

## 14. Documentación generada en sesión

| Documento | Contenido |
|-----------|-----------|
| [DEPLOY.md](./DEPLOY.md) | Guía de deploy (Railway + Vercel + Supabase), variables, migraciones, smoke test |
| [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) | Este registro — changelog vivo de fixes |

---

## 15. Pendiente / deploy

Antes de producción, revisar [DEPLOY.md](./DEPLOY.md):

- [ ] **Commit + push API** (`master`) — incluye fix contacto reparación (§18) si aún está local
- [ ] **Commit + push frontend** (`main`) — WhatsApp recogido (§20) si aún está local
- [ ] Variables Railway (JWT, Resend, Supabase, Admin, `App__PublicUrl`, etc.)
- [ ] `dotnet ef database update` en Supabase prod (puerto **5432**)
- [ ] `Scripts/verify-schema.sql` — columnas de ventas + historial de 4 migraciones mayo 2026
- [ ] `Scripts/apply-pending-migrations.sql` — solo si verify falla
- [ ] Smoke test: login, dashboard, **orden con contacto repetido**, venta con descuento + abonos, email factura
- [ ] Opcional: `/health`, desactivar Swagger en prod, `vercel.json` para SPA

### Comandos rápidos pre-deploy

```powershell
cd MiNegocioCR.Api
dotnet test

cd ../mi-negociocr-frontend
npm run build
```

**Esperado:** 140 tests API, build frontend sin errores TypeScript (warnings de budget CSS son aceptables).

### Ubicación de esta documentación

Los archivos `.md` de registro viven en la carpeta workspace `Mi-negociocr/` (monorepo local). Copias de respaldo en `MiNegocioCR.Api/docs/` para incluirlos en el repo Git de la API.

---

## Cómo agregar un fix a este documento

Copiar esta plantilla al final del archivo:

```markdown
## N. [Área] — Título corto

### Síntoma
Qué veía el usuario.

### Causa
Por qué ocurría.

### Fix
| Archivo | Cambio |
|---------|--------|
| `ruta/archivo` | Descripción |

### Verificación
Cómo confirmar que quedó resuelto.
```

---

*Última actualización: 27 mayo 2026 — deploy prep + contactos + WhatsApp recogido*
