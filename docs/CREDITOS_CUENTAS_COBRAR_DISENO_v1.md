# Créditos y cuentas por cobrar — Diseño funcional v1.1 (MiNegocioCR)

> **Spec oficial.** Diseño acordado junio 2026; **implementado** (fases 0–4, jun 2026).  
> Deploy: [CAMBIOS_CREDITOS_JUNIO_2026.md](./CAMBIOS_CREDITOS_JUNIO_2026.md).  
> Complementa ventas (cobro inmediato), reparaciones (abonos por orden) y [PEDIDOS_INTERNET_DISENO_v1.md](./PEDIDOS_INTERNET_DISENO_v1.md).

**Convención:** en código MiNegocioCR el tenant es `BusinessId` (no `TenantId`). Toda consulta y mutación debe filtrar por `BusinessId` del JWT / sesión.

---

## Objetivo

Módulo de **Créditos** para que cada negocio registre deudas de clientes, abonos, saldos pendientes e historial completo de cuentas por cobrar.

Integración con arquitectura actual:

| Sistema existente | Uso en Créditos |
|-------------------|-----------------|
| `Contact` (Customers) | Un cliente = una cuenta corriente |
| Inventario / variantes | Cargos con productos (descuenta stock vía `IInventoryService`) |
| `Business` + config | Logo, nombre comercial, SMTP, correos |
| `IEmailService` | Envío **manual** de correos (HTML desde frontend) |
| `RepairOrderContactHelper` | Buscar / crear contacto al abrir cuenta |

Diseño **mobile first** (desde 320px), misma UX que reparaciones / pedidos internet.

**v1.1:** sin campos `CreatedBy` / `UpdatedBy` (solo timestamps). Auditoría de usuario queda para versión futura.

---

## Filosofía del módulo

**No** hay múltiples créditos independientes por cliente.

Cada cliente tiene **una sola cuenta corriente de crédito**. Compras, abonos y ajustes se apilan en la misma cuenta con saldo acumulado.

Ejemplo:

```text
Juan Pérez

01 Jun — Crédito (3 líneas)     +₡115,000  → Saldo ₡115,000
20 Jun — Abono ₡100,000 (debe ₡85,000)    → Saldo ₡0, Vuelto ₡15,000
```

Comportamiento típico: pulperías, talleres, ferreterías, salones, comercios de barrio.

---

## Apertura de cuenta

**Sin** pantalla “crear cuenta vacía”. Al registrar el **primer cargo** o abono para un contacto:

1. Resolver contacto (`RepairOrderContactHelper`).
2. Si no existe `CreditAccount` para `(BusinessId, ContactId)` → crearla.
3. Registrar transacción.

---

## Cargos (movimientos tipo Crédito)

### Varias líneas por cargo

Un **cargo** es una `CreditTransaction` tipo `Credito` con **varias** `CreditTransactionLine`:

```text
20 Jun 2026 — Crédito
  Laptop Dell       qty 1  ₡100,000  (+5% línea)
  Mouse Logitech    qty 1  ₡10,500
  Mano de obra      —      ₡5,000
  Total cargo:      ₡115,500
  Saldo:            ₡115,500
```

### Línea de inventario

- Selección desde catálogo / variantes.
- Precio base = **precio de venta** de la variante.
- **Recargo crédito % por línea** (ej. 5%) → `UnitPriceCrc = base × (1 + markupPercent/100)`.
- Precio **editable** manualmente después del cálculo.
- Al confirmar: `DecreaseStockAsync` (sin stock → error, igual que ventas).

### Concepto libre (sin inventario)

Ejemplos: Servicio técnico, Mano de obra, Instalación, Transporte, Diagnóstico, Otros.

No afectan stock. `CatalogVariantId` null.

---

## Abonos y vuelto

- Monto abonado siempre **positivo** en `AmountCrc`; tipo `Abono`.
- Saldo **nunca &lt; 0**.
- Si abono &gt; saldo pendiente:
  - `AppliedToBalanceCrc` = saldo antes del abono
  - `ChangeGivenCrc` = excedente (**vuelto**, solo informativo)
  - `NewBalanceCrc` = 0
- **No** modificar transacciones anteriores.

---

## Convención de montos (opción A)

`AmountCrc` en cabecera de transacción: **siempre positivo**. El **tipo** define el efecto:

| Tipo | Efecto en saldo |
|------|-----------------|
| `Credito` | Suma deuda (total del cargo) |
| `Abono` | Resta deuda (monto abonado) |
| `Renovacion`, `Ajuste`, etc. | Fases posteriores; sin cambio de saldo o según regla explícita |

---

## Modelo de datos (v1.1)

### `CreditAccount` — único `(BusinessId, ContactId)`

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `ContactId` | Índice único compuesto |
| `AccountNumber` | Legible (ej. `CR-YYYYMMDD-###`) |
| `Status` | `Activo`, `Pagado`, `Cancelado` (ver estados) |
| `CurrentBalanceCrc` | Snapshot saldo actual |
| `TotalChargedCrc` | Suma histórica de cargos (métrica) |
| `PaymentCommitmentDate` | Solo seguimiento; sin cron |
| `Notes` | Observaciones de la cuenta |
| `CreatedAt`, `UpdatedAt`, `PaidAt`, `CancelledAt` | Timestamps |

### `CreditTransaction` — historial inmutable (cabecera)

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `CreditAccountId`, `ContactId` | |
| `TransactionType` | Enum |
| `AmountCrc` | Positivo; total cargo o monto abonado |
| `AppliedToBalanceCrc` | Nullable; en abono, monto aplicado a deuda |
| `ChangeGivenCrc` | Nullable; vuelto en abono |
| `Description` | Resumen opcional |
| `PreviousBalanceCrc`, `NewBalanceCrc` | Snapshot |
| `Notes` | Observación del movimiento |
| `CreatedAt` | |

Líneas de cargo en tabla hija **`CreditTransactionLine`**:

| Campo | Notas |
|-------|--------|
| `Id`, `CreditTransactionId` | |
| `SortOrder` | |
| `LineKind` | `Inventory` / `FreeConcept` |
| `CatalogVariantId` | Nullable |
| `ConceptName` | Nombre producto o concepto |
| `Quantity` | Default 1 |
| `BaseUnitPriceCrc` | Precio venta variante o manual |
| `CreditMarkupPercent` | % recargo por línea (0 si no aplica) |
| `UnitPriceCrc` | Precio final unitario |
| `LineTotalCrc` | `UnitPriceCrc × Quantity` |

### `CreditCommunication` (fase 1 mínima / fase 3 ampliada)

| Campo | Notas |
|-------|--------|
| `Id`, `BusinessId`, `CreditAccountId`, `ContactId` | |
| `CommunicationType` | Correo, Llamada, WhatsApp, Visita, Otro |
| `Notes` | |
| `CreatedAt` | |

### Fases posteriores

- `CreditCommitmentRenewal`, consolidación, reportes export.

---

## Estados de cuenta

### Guardados en BD (v1)

| Estado | Regla |
|--------|--------|
| `Activo` | `CurrentBalanceCrc` &gt; 0 |
| `Pagado` | `CurrentBalanceCrc` = 0 |
| `Cancelado` | Solo si saldo = 0 (archivo administrativo; opcional en UI) |

### Solo UI (filtros / badges)

| Badge | Regla |
|-------|--------|
| Parcial | Saldo &gt; 0 y existen abonos previos |
| Vencido | Saldo &gt; 0 y `PaymentCommitmentDate` &lt; hoy |

**No cancelar** cuentas con deuda pendiente: si el cliente desaparece, la cuenta sigue **Activa** (o badge Vencido) para consulta histórica.

---

## Fecha compromiso

- Campo `PaymentCommitmentDate` en cuenta.
- Editable desde detalle / `PATCH commitment`.
- Sin recordatorios automáticos en v1.

---

## Correos (v1 — solo manuales)

**Sin** envío automático al crear cargo o abonar.

Acción **Enviar correo** en detalle (patrón `send-email` de pedidos internet / reparaciones):

1. Modal: destino, asunto, mensaje, vista previa HTML.
2. `POST .../send-email` → `IEmailService`.
3. Registrar `CreditCommunication` tipo Correo.

HTML: logo y datos del tenant; fallback **MiNegocioCR** (`mergePrintBusiness`, `mi-negociocr-print.branding.ts`).

Plantillas sugeridas (asunto / contenido): creación, nuevo cargo, abono, recordatorio, pagado — ver tabla en sección original; todas disponibles vía envío manual.

---

## UI / rutas (v1)

Menú sidebar: **Créditos**.

| Ruta | Vista |
|------|--------|
| `/credits` | Lista unificada con tabs/filtros: **Activos**, **Pagados**, **Vencidos** |
| `/credits/:accountId` | Detalle, timeline, agregar cargo, abonar, correo manual |

**No** `/credits/clients` en v1 (misma data que activos con otro orden).

Patrones: `ContactsSearchService`, lista + detalle (reparaciones), cargos multi-línea (pedidos internet).

---

## API `/api/credit-accounts` (fase 1)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `business/{businessId}` | Listar cuentas (`filter`: active, paid, overdue; `search`) |
| GET | `{businessId}/{id}` | Detalle + transacciones (paginadas) |
| POST | `{businessId}/charges` | Primer cargo o cargo en cuenta existente (auto-crea cuenta) |
| POST | `{businessId}/{id}/charges` | Cargo adicional en cuenta |
| POST | `{businessId}/{id}/payments` | Abono (con vuelto) |
| PATCH | `{businessId}/{id}/commitment` | Fecha compromiso |
| POST | `{businessId}/{id}/send-email` | Correo manual (HTML cliente) |

Fase 2+: consolidación, reportes, comunicaciones CRUD, cancelar (solo saldo 0).

---

## Implementación por fases (acordado)

| Fase | Contenido |
|------|-----------|
| **0** | Documentación v1.1 ✅ |
| **1** | EF + API + UI lista/detalle, cargos multi-línea, abonos, inventario, correo manual | ✅ |
| **2** | POS **“Vender a crédito”**, dashboard KPIs, print | ✅ |
| **3** | Comunicaciones ampliadas, renovación en historial | ✅ |
| **4** | Reportes PDF/Excel, consolidación, archivar, tab Archivadas | ✅ |

---

## Relación con módulos existentes

| Módulo | Diferencia |
|--------|------------|
| Ventas (`Sales`) | Cobro al momento; fase 2 enlaza “vender a crédito” |
| Reparaciones | Abonos **por orden**; no cuenta única por cliente |
| Pedidos Internet | Pedido proxy USD/CRC; no fiado local acumulado |

---

## Seguridad

- Filtrar **siempre** por `BusinessId`.
- **Prohibido** eliminar filas de `CreditTransaction` / líneas.
- Validar `businessId` de ruta vs JWT (mejora global pendiente).

---

*Última actualización: 5 junio 2026 — fases 0–4 implementadas*
