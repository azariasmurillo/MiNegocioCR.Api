# Créditos y cuentas por cobrar — Diseño funcional v1 (MiNegocioCR)

> **Spec oficial (backlog).** Aún **no implementado** en API ni frontend (junio 2026).  
> Complementa ventas (cobro inmediato), reparaciones (abonos por orden) y [PEDIDOS_INTERNET_DISENO_v1.md](./PEDIDOS_INTERNET_DISENO_v1.md).

**Convención:** en código MiNegocioCR el tenant es `BusinessId` (no `TenantId`). Toda consulta y mutación debe filtrar por `BusinessId` del JWT / sesión.

---

## Objetivo

Módulo de **Créditos** para que cada negocio registre deudas de clientes, abonos, saldos pendientes e historial completo de cuentas por cobrar.

Integración con arquitectura actual:

| Sistema existente | Uso en Créditos |
|-------------------|-----------------|
| `Contact` (Customers) | Un cliente = una cuenta corriente |
| Inventario / variantes | Movimientos tipo crédito con productos (descuenta stock) |
| `Business` + config | Logo, nombre comercial, SMTP, correos |
| `IEmailService` | Notificaciones y recordatorios manuales |
| Notificaciones (patrón reparaciones / pedidos internet) | Eventos de estado y pagos |
| Auditoría | `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` |

Diseño **mobile first** (desde 320px), misma UX que reparaciones / pedidos internet.

---

## Filosofía del módulo

**No** hay múltiples créditos independientes por cliente.

Cada cliente tiene **una sola cuenta corriente de crédito**. Compras, abonos y ajustes se apilan en la misma cuenta con saldo acumulado.

Ejemplo:

```text
Juan Pérez

01 Jun — Laptop Dell        +₡100,000  → Saldo ₡100,000
15 Jun — Mouse Logitech     +₡10,000   → Saldo ₡110,000
20 Jun — Abono              −₡25,000   → Saldo ₡85,000
05 Jul — Monitor            +₡80,000   → Saldo ₡165,000
```

Comportamiento típico: pulperías, talleres, ferreterías, salones, comercios de barrio.

---

## Menú principal (frontend)

Nuevo ítem de sidebar:

```text
Créditos
```

Subsecciones / vistas:

| Vista | Descripción |
|-------|-------------|
| Créditos activos | Cuentas con saldo > 0 |
| Clientes con crédito | Listado por contacto |
| Créditos pagados | Saldo = 0 (histórico) |
| Reportes | Filtros + export PDF / Excel |

Rutas sugeridas (propuesta):

| Ruta | Vista |
|------|--------|
| `/credits` | Créditos activos (lista principal) |
| `/credits/clients` | Clientes con crédito |
| `/credits/paid` | Créditos pagados |
| `/credits/reports` | Reportes |
| `/credits/:accountId` | Detalle cuenta + historial |
| `/credits/:accountId/print` | Vista previa / impresión (opcional fase 2) |

---

## Crear / abrir crédito

Flujo similar a **Órdenes de reparación** y **Pedidos Internet**.

### Cliente

- Buscar contacto existente (mín. 2 caracteres, `ContactsSearchService`).
- Crear cliente rápido si no existe.

Campos:

- Nombre, teléfono, email, dirección (opcional).

Reglas:

- Si el contacto existe → usar su **cuenta de crédito** (crear cuenta si es la primera vez).
- Si no existe → crear `Contact` y cuenta en la misma operación.

---

## Agregar movimiento de crédito

### Productos del inventario

- Selección desde catálogo / variantes existentes.
- Al confirmar: **descontar inventario** + registrar movimiento tipo `Credito`.

### Conceptos libres (sin inventario)

Ejemplos: Servicio técnico, Mano de obra, Instalación, Transporte, Diagnóstico, Otros.

No afectan stock.

---

## Modelo de datos (propuesta)

### `CreditAccount` (una por `Contact` + `BusinessId`)

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `ContactId` | Único `(BusinessId, ContactId)` |
| `AccountNumber` | Número legible (ej. diario `YYYYMMDD###`) |
| `Status` | Ver estados abajo |
| `CurrentBalanceCrc` | Snapshot saldo actual |
| `TotalHistoricalCrc` | Suma histórica de cargos (opcional métrica) |
| `PaymentCommitmentDate` | Fecha compromiso (solo seguimiento) |
| `Notes` | Observaciones |
| `CreatedAt`, `UpdatedAt`, `PaidAt`, `CancelledAt` | |
| Auditoría | `CreatedBy`, `UpdatedBy` |

### `CreditTransaction` (historial inmutable)

**Nunca** sobrescribir filas; cada operación = nueva fila.

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `CreditAccountId`, `ContactId` | |
| `TransactionType` | Enum (ver abajo) |
| `AmountCrc` | Positivo = aumenta deuda; negativo o tipo Abono según diseño de signo |
| `Description` | Texto / referencia producto |
| `InventoryVariantId` | Nullable si es producto |
| `Quantity` | Nullable |
| `PreviousBalanceCrc`, `NewBalanceCrc` | Snapshot en el momento |
| `CreatedAt`, `CreatedBy` | |

**`TransactionType`:**

```text
Credito
Abono
Renovacion
Consolidacion
Ajuste
Cancelacion
PagoCompleto
```

### `CreditCommunication`

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `CreditAccountId`, `ContactId` | |
| `CommunicationType` | Correo, Llamada, WhatsApp, Visita, Otro |
| `Notes` | |
| `CreatedAt`, `CreatedBy` | |

### Tablas auxiliares (fase 2+)

- `CreditCommitmentRenewal` — historial renovación fecha (fecha anterior, nueva, motivo, usuario).
- `CreditConsolidation` — trazabilidad consolidación.

---

## Estados de cuenta

| Estado | Definición |
|--------|------------|
| `Activo` | Deuda pendiente, sin abonos o recién creada |
| `Parcial` | Deuda pendiente con abonos parciales |
| `Renovado` | Fecha compromiso renegociada (registro en historial) |
| `Consolidado` | Movimientos unificados (trazabilidad) |
| `Pagado` | Saldo = 0 |
| `Cancelado` | Cuenta anulada; historial conservado |
| `Vencido` | Superó fecha compromiso (cálculo / flag; sin automatismos) |

---

## Reglas de negocio

### Abonos

- Fecha, monto, observación.
- Recalcula saldo; nueva fila `CreditTransaction` tipo `Abono`.
- **No** modificar transacciones anteriores.

### Fecha compromiso de pago

- Campo `PaymentCommitmentDate`.
- Solo seguimiento; **sin** acciones automáticas por vencimiento en v1.

### Renovación

- Cambiar fecha compromiso + motivo.
- Historial: fecha anterior, nueva fecha, motivo, usuario (`Renovacion`).

### Consolidación

- Unificar movimientos de deuda manteniendo trazabilidad (`Consolidacion`).

### Crédito pagado

- Cuando saldo → 0: estado `Pagado`, registrar fecha y usuario.
- Correo de agradecimiento (plantilla tenant).

### Cancelar crédito

- Estado `Cancelado`; **no** borrar datos.

---

## Historial de movimientos (UI)

Vista tipo línea de tiempo (como ejemplo del spec):

```text
01/06/2026 — Crédito — Laptop Dell — ₡100,000 — Saldo ₡100,000
15/06/2026 — Crédito — Mouse — ₡10,000 — Saldo ₡110,000
20/06/2026 — Abono — ₡25,000 — Saldo ₡85,000
```

---

## Recordatorio manual (v1)

**No** recordatorios automáticos programados.

Acción **Enviar recordatorio** en detalle de cuenta:

1. Modal: correo destino, asunto, mensaje, vista previa.
2. Enviar → `IEmailService` + fila `CreditCommunication` tipo Correo.
3. HTML con logo y datos del **negocio** (`Business` / config), mismo patrón que pedidos internet y facturas — `mergePrintBusiness` + `mi-negociocr-print.branding.ts` en frontend.

---

## Dashboard (extensión)

KPIs sugeridos (filtro `BusinessId`):

- Créditos activos (count)
- Monto pendiente total
- Clientes morosos (compromiso vencido + saldo > 0)
- Abonos del mes
- Créditos pagados (periodo)
- Top 10 clientes con mayor deuda
- Monto total pendiente

API propuesta: `/api/dashboard/{businessId}/credits-summary` o ampliar `DashboardController`.

---

## Reportes

Filtros: cliente, rango fechas, estado, usuario, monto.

Export: PDF, Excel (fase 2).

---

## Correos (plantillas)

Todos usan **config del tenant** (`LogoUrl`, nombre comercial, teléfono, `PublicEmail`, SMTP del `Business`).  
Fallback de plataforma: **MiNegocioCR** (ver `mi-negociocr-print.branding.ts`), no marca de un tenant demo.

| Evento | Asunto sugerido |
|--------|-----------------|
| Creación / primer crédito | Se ha registrado un crédito a su nombre |
| Nuevo movimiento | Movimiento en su cuenta — nuevo saldo |
| Abono | Abono registrado — saldo pendiente |
| Renovación | Actualización fecha de pago comprometida |
| Recordatorio manual | Recordatorio de saldo pendiente |
| Pagado | Crédito cancelado exitosamente (saldo ₡0) |

Contenido mínimo por correo: cliente, fechas, montos, saldo actual, fecha compromiso (si aplica).  
Recordatorio: botón/enlace **Contactar negocio** (teléfono / email del tenant).

Envío manual de HTML desde frontend (patrón `send-email` de reparaciones / pedidos internet) + notificaciones automáticas opcionales vía `EnableEmailNotifications`.

---

## Seguridad y multi-tenant

- Filtrar **siempre** por `BusinessId` en queries y comandos.
- Validar que `businessId` de ruta coincida con JWT (mejora pendiente global en API).
- Auditoría en entidades principales.
- **Prohibido** eliminar historial de `CreditTransaction`; solo estados terminales.

---

## API (propuesta `/api/credit-accounts`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `business/{businessId}` | Listar cuentas (filtros estado, búsqueda) |
| GET | `{businessId}/{id}` | Detalle + transacciones paginadas |
| POST | `{businessId}` | Crear cuenta / primer movimiento |
| POST | `{businessId}/{id}/transactions` | Agregar crédito (producto o concepto) |
| POST | `{businessId}/{id}/payments` | Registrar abono |
| PATCH | `{businessId}/{id}/status` | Cambios estado / cancelar |
| PATCH | `{businessId}/{id}/commitment` | Renovación fecha |
| POST | `{businessId}/{id}/consolidate` | Consolidación |
| POST | `{businessId}/{id}/communications` | Registrar llamada / WhatsApp / etc. |
| POST | `{businessId}/{id}/send-email` | Recordatorio / plantilla (HTML cliente) |
| GET | `business/{businessId}/reports` | Datos reportes (fase 2) |

---

## Frontend (propuesta)

- Feature: `src/app/features/credits/`
- Servicios: `credit-accounts.service.ts`, mapper, calculadora de saldo (solo validación UI).
- Patrones: lista + detalle, diálogo gestionar, `PrintPreviewDialog` opcional, `ContactsSearchService`.
- Menú sidebar: **Créditos** con subnavegación o tabs internas.

---

## Implementación por fases

| Fase | Contenido |
|------|-----------|
| **0** (actual) | Este documento + índices en repos |
| **1** | Entidades, migración EF, CRUD cuenta + transacciones + abonos, estados, API |
| **2** | UI lista/detalle, inventario en movimientos, dashboard KPI básicos |
| **3** | Correos automáticos + recordatorio manual + comunicaciones |
| **4** | Reportes PDF/Excel, renovación/consolidación avanzada, print |

---

## Relación con módulos existentes

| Módulo | Diferencia |
|--------|------------|
| Ventas (`Sales`) | Cobro al momento; no cuenta corriente persistente |
| Reparaciones | Abonos **por orden**; no cuenta única por cliente |
| Pedidos Internet | Pedido proxy USD/CRC; no fiado local acumulado |

---

*Última actualización: 4 junio 2026 — spec v1 backlog*
