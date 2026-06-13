# Inventario — biblioteca de variantes y catálogo de dimensiones (Junio 2026)

**Deploy:** pendiente (13 jun 2026)  
Complementa [CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md](./CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md).

---

## Resumen

| Área | Cambio |
|------|--------|
| **Catálogo de dimensiones** | 10 dimensiones estándar + opción «Personalizada…»; máx. **3 dimensiones** por producto |
| **Biblioteca de valores** | Tabla tenant `BusinessDimensionValues`; presets del sistema en FE; valores reutilizables por negocio |
| **Normalización** | Case-insensitive, dedupe y reglas por dimensión (Marca, Talla, Capacidad, etc.) en API y al agregar valor nuevo en FE |
| **UI variantes** | Dropdown multi-select (máx. 3 valores/dimensión) + campo «Valor nuevo» debajo; sin textarea libre ni nube de chips |
| **Textos** | Presentación → **Variante** en modales, listado y mensajes |

---

## Decisiones de producto

1. Lista **semi-cerrada** de dimensiones (no 100% libre): catálogo estándar + escape personalizado.
2. **Biblioteca tenant** para **todas** las dimensiones (no solo Marca).
3. Presets de sistema en FE (Color, Talla, Marca con ANNEX/LOGI/UNNO/Red Dragon/DELL/HP, etc.) + valores guardados del negocio vía API.
4. Valores nuevos se **normalizan** al guardar (ej. `hp` → `HP`, `azul oscuro` → `Azul Oscuro`, `128gb` → `128 GB`).
5. **Sin migración automática** de datos viejos mal cargados; limpieza manual en tenants afectados (ej. Joyca Tech).

---

## API

### Migración

```powershell
cd MiNegocioCR.Api
dotnet ef database update
```

Migración: **`20260613202954_AddBusinessDimensionValues`**

### Endpoints nuevos

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/dimension-library/catalog` | Dimensiones estándar + `maxDimensionsPerProduct` |
| `GET` | `/api/dimension-library/{businessId}?dimension=Marca` | Valores de biblioteca del tenant para una dimensión |

### Entidad y reglas

| Archivo | Rol |
|---------|-----|
| `Domain/Entities/BusinessDimensionValue.cs` | Valor reutilizable por tenant y dimensión |
| `Application/Common/CatalogDimensionRules.cs` | Whitelist dimensiones, max 3, normalización |
| `Infrastructure/Persistence/Repositories/BusinessDimensionValueRepository.cs` | CRUD biblioteca |
| `Application/UseCases/Catalog/GetDimensionLibraryUseCases.cs` | Casos de uso GET |
| `API/Controllers/DimensionLibraryController.cs` | Controller |

### Use cases actualizados

- `CreateOptionUseCase` / `UpdateOptionUseCase` — whitelist, `IsCustomDimension`, max 3, sin duplicar dimensión.
- `CreateOptionValueUseCase` / `UpdateOptionValueUseCase` — normalización, anti-duplicado, upsert en biblioteca.
- `CreateVariantUseCase` — exige **exactamente 1 valor por cada dimensión activa** del producto.

### DTOs

- `CreateCatalogOptionRequestDto.IsCustomDimension`
- `UpdateOptionRequestDto.IsCustomDimension`
- `BusinessDimensionValueDto`, `CatalogDimensionCatalogDto`

### Tests

```powershell
dotnet test MiNegocioCR.Tests --filter "CatalogDimensionRules|CreateVariantUseCase"
```

---

## Frontend

### Componentes y utilidades nuevos

| Archivo | Rol |
|---------|-----|
| `dimension-value-picker/*` | Multi-select biblioteca (máx. 3) + agregar valor normalizado |
| `dimension-catalog.constants.ts` | Dimensiones estándar, presets, token `__custom__` |
| `dimension-library.service.ts` | Consume API biblioteca |
| `normalize-dimension-value.util.ts` | Normalización FE alineada con API |

### Pantallas tocadas

| Archivo | Cambio |
|---------|--------|
| `product-quick-add-dialog/*` | Dimensión dropdown + picker inline; botón «+ Dimensión» centrado |
| `presentation-add-dialog/*` | Agregar dimensión/valor con picker; textos variante |
| `product-detail-dialog/*`, `inventory-list/*` | Renombre presentación → variante |
| `inventory-orchestrator.service.ts` | `isCustomDimension` al crear opciones |
| `presentation-matrix.util.ts` | Drafts con `values[]` e `isCustomDimension` |

### UX — Agregar producto (variantes)

- Fila: **Dimensión** | **Valores** (dropdown multi, máx. 3) | **Quitar**
- Debajo del dropdown: «¿No está en la lista?» → **Valor nuevo** + **Agregar**
- Botón **+ Dimensión** centrado; espaciado respecto a stock inicial

### Presets Marca (sistema)

`ANNEX`, `LOGI`, `UNNO`, `Red Dragon`, `DELL`, `HP` (+ valores del tenant + libre controlado)

---

## Smoke test

- [ ] Migración aplicada en BD local/prod
- [ ] Agregar producto → variantes Sí → Marca + Color desde dropdown (máx. 3 c/u)
- [ ] Valor nuevo `hp` en Marca → guarda como `HP`
- [ ] Agregar variante → «+ Valor nuevo…» → picker biblioteca
- [ ] Crear variante exige un valor por cada dimensión activa
- [ ] Duplicado case-insensitive rechazado (`Blanco` vs `BLANCO`)

---

## Pendiente (fuera de este commit)

- UI **eliminar variantes** en detalle producto (limpieza Joyca Tech)
- Enriquecer select de valores existentes en modal agregar variante con biblioteca mergeada

---

*Última actualización: 13 junio 2026*
