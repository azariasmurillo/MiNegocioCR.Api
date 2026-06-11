# Inventario API — Cambios Junio 2026 (Sprint 4)

Complementa el changelog frontend: [CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md](../../mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md)

---

## Endpoints nuevos / ampliados

### `PATCH /api/variants/{id}/toggle`

Activa o desactiva una **presentación** (`CatalogVariant`).

**Body:**

```json
{
  "businessId": "guid",
  "isActive": false
}
```

**Comportamiento:**

- `isActive: false` → `StockQuantity = 0` + movimiento de inventario «Presentación desactivada»
- `isActive: true` → solo cambia flag (no restaura stock anterior)

**Archivos:**

- `Application/UseCases/Repository/ToggleVariantStatusUseCase.cs`
- `Application/DTOs/ToggleVariantStatusRequestDto.cs`
- `Application/Interfaces/Repositories/IToggleVariantStatusUseCase.cs`
- `API/Controllers/VariantController.cs` — registrar `_toggleVariantStatus` en constructor

---

## DTOs de listado (`CatalogVariantListItemDto`)

Campos usados por el grid y detalle:

| Campo | Uso FE |
|-------|--------|
| `ProfitMargin` | Margen override por variante |
| `EffectiveProfitMargin` | Margen efectivo (variante o default negocio) |
| `IsActive` | Presentación activa/inactiva |
| `PrimaryImageUrl` | Thumbnail en grid y detalle |
| `CostPrice` | Edición y detalle |

**Use cases:** `GetVariantsByCatalogItemUseCase`, `GetVariantsByBusinessUseCase`

---

## Actualización variante (`PUT /api/variants/{id}`)

- `SetProfitMargin: true` persiste `ProfitMargin` en variante
- Precio recalculado desde costo + margen + IVA cuando aplica

---

## Bugs corregidos

| Bug | Causa | Fix |
|-----|-------|-----|
| `NullReferenceException` al toggle | Falta inyección en `VariantController` | Constructor + campo `_toggleVariantStatus` |
| 400 al desactivar presentación | Transacción/repositorio | Simplificar `ToggleVariantStatusUseCase`; stock vía `IInventoryRepository` |
| Error al actualizar variante tracked | `Update()` en entidad ya tracked | `VariantRepository.UpdateAsync` — no llamar `Update()` si no está detached |

---

## Catálogo (`GET /catalog/{businessId}`)

- Query `includeInactive=true` — lista incluye ítems inactivos (FE filtra en pestaña Inactivos)
- `Type` serializado como `"product"` / `"service"` (camelCase enum)

**FE:** `normalizeCatalogItemType()` en `catalog-item.util.ts`

---

## Migraciones

**Ninguna nueva** para Sprint 4 inventario. Usa:

- `ProfitMargin` en `CatalogVariants` (migración mayo 2026)
- `IsActive` en catálogo y variantes (existente)

Antes de deploy en prod: `dotnet ef database update` o scripts idempotentes según [DEPLOY.md](./DEPLOY.md).

---

## Tests

```bash
cd MiNegocioCR.Api
dotnet test
```

Esperado: **204** tests passing (incluye `ToggleVariantStatusUseCaseTests`, `UpdateVariantUseCaseTests`).

**Nota:** Si `dotnet test` falla con archivo bloqueado, detener `dotnet run` local de la API y reintentar.

---

## Smoke test API post-deploy

```http
GET /api/variants/business/{businessId}?includeInactive=true
PATCH /api/variants/{variantId}/toggle
PUT /api/variants/{variantId}  (SetProfitMargin)
```

Verificar en consola del navegador sin 500 al desactivar presentación desde Inventario.
