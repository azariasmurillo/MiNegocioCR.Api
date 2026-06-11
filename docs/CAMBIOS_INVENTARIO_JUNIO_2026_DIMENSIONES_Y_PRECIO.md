# Inventario — dimensiones, quick-add y errores (Junio 2026)

**Deploy:** 11 junio 2026  
**Frontend:** `3b1f700` (`main`)  
**API:** `f517a01` (`master`)

Complementa [CAMBIOS_INVENTARIO_POST_SPRINT4_JUNIO_2026.md](./mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_POST_SPRINT4_JUNIO_2026.md) y cierra el rediseño operativo de inventario antes del **marketplace**.

---

## Resumen

| Área | Cambio |
|------|--------|
| **Detalle producto** | Sin sección «Dimensiones»; sin botón Eliminar presentación |
| **Agregar presentación** | Renombrar/eliminar dimensión; editar/eliminar valor al lado del selector |
| **Errores API** | Mensajes en español cuando dimensión/valor está en uso |
| **Agregar producto** | Margen % por fila en paso 2 (modo calculado); costo arriba de precio en monto fijo |
| **Créditos** | Modal «+ Registrar cargo», búsqueda auto (commit previo `87d750a`) |

---

## Frontend — archivos

| Archivo | Cambio |
|---------|--------|
| `presentation-add-dialog/*` | Gestión dimensión/valor; íconos editar + eliminar junto al select |
| `product-detail-dialog/*` | Quitada UI de dimensiones y eliminar presentación |
| `product-quick-add-dialog/*` | Columna Margen % en matriz; validación por fila |
| `inventory-api-error.ts` | **Nuevo** — parseo errores HTTP → mensajes ES |
| `presentation-matrix.util.ts` | `profitMarginPercent` por fila en matriz |
| `inventory-orchestrator.service.ts` | Margen por presentación al crear producto multi-SKU |

---

## API

| Archivo | Cambio |
|---------|--------|
| `DeleteOptionValueUseCase.cs` | Mensaje: «Hay presentaciones que usan este valor…» |
| `DeleteOptionUseCase.cs` | Mensaje: «Hay presentaciones que usan esta dimensión…» |
| `DeleteVariantUseCase.cs` | Bloqueo por créditos y reparaciones (commit previo `b133fcf`) |

---

## UX acordada — Agregar presentación

- Nombre dimensión: lápiz (renombrar) + basurero (eliminar dimensión).
- Selector de valor: lápiz + basurero rojo para el valor seleccionado.
- Sin chips/burbujas de todos los valores.

---

## UX acordada — Agregar producto (multi-presentación)

- **Monto fijo:** costo base opcional en paso 1; precio por fila en paso 2.
- **Calculado:** costo base + margen en paso 1; en paso 2 columna **Margen %** editable por presentación.
- **Stock inicial por presentación** visible en paso 1 (valor default para la matriz).

---

## Smoke test

- [ ] Agregar presentación → eliminar valor en uso → snackbar en español
- [ ] Agregar presentación → eliminar dimensión «Prueba» si ninguna variante la usa
- [ ] Agregar producto con presentaciones → calculado → margen distinto por fila en paso 2
- [ ] Detalle producto: solo Editar / Fotos por presentación (sin Eliminar)

---

*Última actualización: 11 junio 2026*
