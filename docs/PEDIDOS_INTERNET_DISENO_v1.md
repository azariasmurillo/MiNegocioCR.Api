# Pedidos Internet — Diseño funcional v1 (MiNegocioCR)

> **Spec oficial** para compras asistidas (Amazon / internet).  
> Complementa reparaciones (equipos) y tienda digital pública (`StoreOrder`).

---

## Objetivo

Registrar pedidos proxy: el cliente elige productos (USD), el negocio aplica tipo de cambio **interno**, costos en colones, adelantos y estados hasta entrega. Comprobante al cliente con líneas en **USD** y totales en **₡** (sin mostrar tipo de cambio).

---

## Estados

| API | UI | Correo automático |
|-----|-----|-------------------|
| `Created` | Creada | No |
| `Purchased` | Comprada | Sí → cliente |
| `Received` | Recibida | Sí → cliente |
| `Delivered` | Entregada | No |
| `Cancelled` | Cancelada | Nota reembolso (manual, ₡ / adelantos) |

Transiciones: `Created → Purchased → Received → Delivered`; cancelar desde cualquier estado no terminal.

**Edición:** permitida en cualquier momento (líneas, TC, costos, adelantos).

---

## Modelo de datos

### `InternetOrder`

- `OrderNumber`, `BusinessId`, `ContactId`, `Status`
- `ExchangeRateApplied` — **solo staff / API interna**
- `InternationalShippingCost`, `LocalCourierCost`, `ServiceFee` (CRC)
- Snapshots: `LinesTotalUsd`, `LinesTotalCrc`, `SubtotalCrc`, `TotalAdvancesCrc`, `BalanceDueCrc`
- `CustomerNotes`, `InternalNotes`, `RefundNote`
- `ExternalOrderId`, `TrackingNumber` (opcional)
- Timestamps: `CreatedAt`, `PurchasedAt`, `ReceivedAt`, `DeliveredAt`, `CancelledAt`

### `InternetOrderLine`

- `ProductName`, `ProductUrl`, `UnitPriceUsd`, `Quantity`
- `LineTotalUsd`, `LineTotalCrc` (snapshot)
- `SortOrder`

### `InternetOrderAdvance`

- `AmountCrc`, `PaidAt`, `Method`, `Notes`

---

## Cálculos

```
LineTotalUsd = UnitPriceUsd × Quantity
LineTotalCrc = LineTotalUsd × ExchangeRateApplied
LinesTotalUsd = Σ LineTotalUsd
LinesTotalCrc = Σ LineTotalCrc
SubtotalCrc = LinesTotalCrc + InternationalShipping + LocalCourier + ServiceFee
TotalAdvancesCrc = Σ advances
BalanceDueCrc = max(0, SubtotalCrc - TotalAdvancesCrc)
```

---

## Documento al cliente

### Bloque USD (líneas)

- Nombre, link, cantidad × precio USD, total línea USD  
- **Subtotal productos (USD)**

### Bloque CRC (sin tipo de cambio)

- Total productos (₡), traída, flete, servicio, **Total**, tabla **Adelantos**, **Saldo**

Correos Comprada/Recibida: mismo formato. Reembolso: solo ₡ según adelantos registrados.

---

## API (`/api/internet-orders`)

| Método | Ruta |
|--------|------|
| POST | `{businessId}` |
| PUT | `{businessId}/{id}` |
| GET | `business/{businessId}` |
| GET | `{businessId}/{id}` |
| PATCH | `{businessId}/{id}/status` |

Fase siguiente: `send-document`, `send-refund-note`, print frontend.

---

## Frontend

- Ruta: `/internet-orders`
- Menú: **Pedidos Internet**
- Patrón: lista + crear/editar (como reparaciones)

---

## Implementación por fases

| Fase | Contenido |
|------|-----------|
| **1** (actual) | Entidades, migración, CRUD, estados, cálculos, UI base |
| 2 | Print HTML, send-email, notificaciones Comprada/Recibida |
| 3 | Dashboard KPI, reenvío comprobante |

---

*Última actualización: mayo 2026*
