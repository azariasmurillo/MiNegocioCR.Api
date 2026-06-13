# Import ZIP de imágenes de variantes + SKU único por negocio — Diseño v1

> **Spec para desarrollo.** Prerrequisito del marketplace (catálogo visual uniforme).  
> Complementa [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md) y el upload manual actual (`POST /api/variants/{id}/images`).

| Decisión | Valor acordado |
|----------|----------------|
| Match import ZIP | **SKU único por negocio** (no solo por producto) |
| Nombres ZIP | `{SKU}_{1\|2\|3}.{jpg\|jpeg\|png\|webp}` |
| Máx. imágenes | 3 por SKU / variante |
| Formato final | **WebP** 1200×1200 (main), 600×600 (mobile), 300×300 (thumb) |
| Procesamiento | Async por lote (`ImageImportBatch`) |
| IA MVP | **Fase 2** — Fase 1 con ImageSharp + estilo fijo |
| Storage | Supabase (extender paths multi-tamaño) |
| Reemplazo | Solo si `replaceExisting=true` |

**Estado código (jun 2026):** **Sin implementar** — diseño aprobado para sprint.

---

## 1. Problema: SKU hoy no sirve para import masivo

### Comportamiento actual

| Regla | Dónde |
|-------|--------|
| SKU único **por `CatalogItem`** | `VariantRepository.ExistsSkuForCatalogItemAsync` |
| Índice BD en `CatalogVariants.SKU` | Global, **sin** `BusinessId` — no garantiza unicidad tenant |
| Upload imágenes | Manual, 1 variante, PNG/JPEG sin procesar |

Consecuencia: dos productos del mismo negocio pueden tener variante `MOUSE-LOGITECH-M185` → el ZIP **no puede** resolver variante de forma determinística.

### Decisión cerrada

**SKU obligatorio y único por negocio** (case-insensitive, trim).

- Un `BusinessId` + un SKU normalizado → **como máximo una** `CatalogVariant`.
- Sigue permitiendo el mismo SKU en **negocios distintos** (multi-tenant).
- Variantes sin SKU: permitidas en UI manual, pero **excluidas del import ZIP** (reporte `SkuMissing`).

---

## 2. SKU único por negocio — diseño técnico

### 2.1 Normalización

```csharp
// Application/Common/SkuNormalizer.cs
public static string? NormalizeSku(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return null;
    var trimmed = raw.Trim();
    if (trimmed.Length == 0) return null;
    if (trimmed.Length > 80) throw new ArgumentException("SKU max 80 chars.");
    return trimmed; // comparación siempre ToLowerInvariant en queries
}
```

**Comparación:** `LOWER(TRIM(sku))` — no cambiar mayúsculas guardadas salvo política futura.

### 2.2 Base de datos

**Migración:** `AddUniqueSkuPerBusiness`

1. **Precheck script** (manual en prod si hay duplicados):

```sql
SELECT ci."BusinessId", LOWER(TRIM(v."SKU")) AS sku_key, COUNT(*) 
FROM "CatalogVariants" v
JOIN "CatalogItems" ci ON ci."Id" = v."CatalogItemId"
WHERE v."SKU" IS NOT NULL AND TRIM(v."SKU") <> ''
GROUP BY ci."BusinessId", LOWER(TRIM(v."SKU"))
HAVING COUNT(*) > 1;
```

2. Resolver duplicados en tenants afectados (Joyca Tech, etc.) **antes** de aplicar índice.

3. Índice único parcial PostgreSQL:

```csharp
entity.HasIndex(v => new { v.BusinessId, v.SkuNormalized })
    .IsUnique()
    .HasFilter("\"SKU\" IS NOT NULL AND TRIM(\"SKU\") <> ''");
```

**Opción A (recomendada):** columna persistida `SkuNormalized` en `CatalogVariants`:

```csharp
public Guid BusinessId { get; set; }           // denormalizado desde CatalogItem al crear/actualizar
public string? SkuNormalized { get; set; }     // LOWER(TRIM(SKU)), null si SKU vacío
```

Mantiene join simple en import y evita expresión en índice.

**Opción B:** índice único vía `CREATE UNIQUE INDEX ... ON (business_id, lower(trim(sku)))` sin columna extra — más SQL manual.

### 2.3 API — validación

Reemplazar / complementar `ExistsSkuForCatalogItemAsync` con:

```csharp
Task<bool> ExistsSkuForBusinessAsync(Guid businessId, string sku, Guid? excludeVariantId = null);
Task<CatalogVariant?> FindByBusinessAndSkuAsync(Guid businessId, string sku);
Task<IReadOnlyList<CatalogVariant>> FindAllByBusinessAndSkuAsync(...); // solo import diagnostics
```

**Use cases a tocar:**

| Archivo | Cambio |
|---------|--------|
| `CreateVariantUseCase` | Validar unicidad negocio; set `BusinessId` + `SkuNormalized` |
| `UpdateVariantUseCase` | Idem |
| `CreateVariant` / quick-add orchestration (FE) | Mensaje ES: «Ya existe otra variante con el SKU {sku} en tu negocio» |

**Mantener** check por producto como defensa en profundidad (redundante si negocio es único).

### 2.4 Frontend

| Pantalla | Cambio |
|----------|--------|
| Quick-add / agregar variante | Validación async opcional `GET /variants/business/{id}?search={sku}` o endpoint dedicado |
| Import ZIP (futuro) | Bloquear lote si hay SKUs duplicados en BD pendientes de limpieza |

### 2.5 Criterios de aceptación SKU

- [ ] No se pueden crear dos variantes del mismo negocio con el mismo SKU (case-insensitive).
- [ ] Migración falla con mensaje claro si hay duplicados existentes.
- [ ] `FindByBusinessAndSkuAsync("LAPTOP-HP840G5")` devuelve 0 o 1 fila.
- [ ] Variante sin SKU: creación manual OK; import ZIP la omite con log.

---

## 3. Import ZIP — objetivo marketplace

Uniformidad visual tipo Shopify/Amazon:

- Mismo canvas 1:1, fondo degradado fijo, sombra suave, WebP optimizado.
- Pipeline IA (fase 2): segmentar producto, quitar fondo, **sin alterar** color/logo/forma.
- Fallback ImageSharp (fase 1): canvas + centrado + degradado + sombra.

---

## 4. Convención de nombres en ZIP

### Regex

```regex
^(?<sku>[A-Za-z0-9][A-Za-z0-9._-]{0,78}[A-Za-z0-9]|\d+)_(?<slot>[1-3])\.(?<ext>jpe?g|png|webp)$
```

Case-insensitive en extensión; **SKU en archivo conserva casing** → se matchea contra BD con normalización.

### Ejemplos válidos

| Archivo | SKU | Slot | Primary |
|---------|-----|------|---------|
| `LAPTOP-HP840G5_1.jpg` | `LAPTOP-HP840G5` | 1 | ✅ |
| `MOUSE-LOGITECH-M185_2.webp` | `MOUSE-LOGITECH-M185` | 2 | |
| `HP_3.png` | `HP` | 3 | |

### Inválidos (log, no abortar lote)

| Caso | Status log |
|------|------------|
| `foto.jpg` (sin `_1`) | `InvalidFileName` |
| `SKU_4.jpg` | `InvalidSlot` |
| `SKU_1.gif` | `UnsupportedFormat` |
| Duplicado `SKU_1` ×2 en ZIP | `DuplicateSlotInZip` |

---

## 5. Modelo de datos — imágenes

### 5.1 Extender `CatalogVariantImages`

```csharp
public class CatalogVariantImage
{
    // existentes
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CatalogVariantId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }

    // nuevos
    public int SortOrder { get; set; }              // 1..3 desde sufijo archivo
    public string ImageUrl { get; set; }            // main 1200 webp (compat: PrimaryImageUrl)
    public string? MobileImageUrl { get; set; }     // 600 webp
    public string? ThumbnailImageUrl { get; set; }  // 300 webp
    public Guid? ImportBatchId { get; set; }
    public string? SourceFileName { get; set; }
}
```

**Listados marketplace:** `ThumbnailImageUrl ?? ImageUrl` en grid; `ImageUrl` + galería en detalle.

### 5.2 `ImageImportBatch`

```csharp
public enum ImageImportBatchStatus { Pending, Processing, Completed, CompletedWithErrors, Failed }

public class ImageImportBatch
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string OriginalFileName { get; set; }
    public bool ReplaceExisting { get; set; }
    public bool UseAiProcessing { get; set; }
    public string MarketplaceStyle { get; set; } = "v1";
    public ImageImportBatchStatus Status { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public string? SummaryJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### 5.3 `ImageImportLog`

```csharp
public enum ImageImportLogStatus
{
    Success, SkippedExisting, VariantNotFound, AmbiguousSku,
    InvalidFileName, InvalidSlot, UnsupportedFormat, ProcessingFailed, StorageFailed
}

public class ImageImportLog
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string FileName { get; set; }
    public string? ParsedSku { get; set; }
    public int? SortOrder { get; set; }
    public Guid? CatalogVariantId { get; set; }
    public ImageImportLogStatus Status { get; set; }
    public string? Message { get; set; }
    public int? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 6. API

### 6.1 Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/catalog/variant-images/import-zip` | Sube ZIP → crea batch → **202** `{ batchId }` |
| `GET` | `/api/catalog/variant-images/import-batches/{batchId}` | Estado + contadores |
| `GET` | `/api/catalog/variant-images/import-batches/{batchId}/logs` | Detalle paginado |
| `GET` | `/api/variants/by-sku/{sku}` | *(opcional)* Resolver variante por SKU tenant — útil FE + import |

**Auth:** `[Authorize]` — `businessId` desde claim/token (nunca confiar solo en body).

**Request `import-zip` (multipart):**

| Campo | Tipo | Default |
|-------|------|---------|
| `file` | ZIP | requerido |
| `replaceExisting` | bool | `false` |
| `useAiProcessing` | bool | `false` (fase 2) |
| `marketplaceStyle` | string | `"v1"` |

**Límites configurables (`appsettings`):**

```json
"VariantImageImport": {
  "MaxZipBytes": 104857600,
  "MaxImageBytes": 10485760,
  "MaxFilesPerZip": 500,
  "MaxImagesPerVariant": 3
}
```

### 6.2 Flujo async

```
POST import-zip
  → guardar ZIP temp / Supabase staging
  → ImageImportBatch Pending
  → encolar ImageImportBackgroundService

Worker:
  1. Validar ZIP (bomba, paths ../, solo imágenes)
  2. Extraer + agrupar por SKU
  3. Por cada archivo:
       a. Parse nombre
       b. FindByBusinessAndSkuAsync
       c. Si 0 → log VariantNotFound
       d. Si >1 → log AmbiguousSku (no debe pasar post-migración SKU)
       e. Si slot ocupado y !replaceExisting → SkippedExisting
       f. IProductImageEnhancerService → 3 tamaños WebP
       g. Upload Supabase + upsert CatalogVariantImage
       h. log Success
  4. Batch Completed / CompletedWithErrors
```

Patrón existente: `CampaignQueueBackgroundService` + `IHostedService`.

### 6.3 DTOs respuesta

```csharp
public sealed class ImageImportBatchDto
{
    public Guid Id { get; init; }
    public string Status { get; init; }
    public int TotalFiles { get; init; }
    public int ProcessedFiles { get; init; }
    public int SuccessCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class ImageImportLogDto
{
    public string FileName { get; init; }
    public string? ParsedSku { get; init; }
    public int? SortOrder { get; init; }
    public string Status { get; init; }
    public string? Message { get; init; }
    public Guid? CatalogVariantId { get; init; }
}
```

---

## 7. Procesamiento de imagen

### 7.1 Interfaz

```csharp
public interface IProductImageEnhancerService
{
    Task<ProductImageEnhanceResult> EnhanceAsync(
        Stream input,
        ProductImageEnhanceOptions options,
        CancellationToken ct = default);
}

public sealed class ProductImageEnhanceOptions
{
    public string MarketplaceStyle { get; init; } = "v1";
    public bool UseAi { get; init; }
    public int MainSize { get; init; } = 1200;
    public int MobileSize { get; init; } = 600;
    public int ThumbnailSize { get; init; } = 300;
    public int WebpQuality { get; init; } = 88;
}
```

### 7.2 Implementaciones (orden de rollout)

| Fase | Clase | Notas |
|------|-------|-------|
| **1** | `LocalImageSharpProductImageEnhancerService` | Reutilizar SixLabors; preset `MarketplaceStyleV1` |
| **2** | `CloudinaryProductImageEnhancerService` **o** `ClipdropProductImageEnhancerService` | Un proveedor; cadena try → fallback local |
| **3** | Otros | Solo si hace falta |

**`MarketplaceStyleV1` (constantes):**

- Canvas 1200×1200, fondo `#F7F9FB` → `#EEF2F6` degradado vertical suave
- Producto ~78% del canvas, centrado
- Sombra elíptica 8% opacidad bajo el producto
- WebP quality 88

### 7.3 Storage paths Supabase

```
variant/{variantId}/{imageId}/main.webp
variant/{variantId}/{imageId}/mobile.webp
variant/{variantId}/{imageId}/thumb.webp
```

Extender `IVariantImageStorageService`:

```csharp
Task<VariantImageStorageResult> UploadProcessedAsync(
    Guid catalogVariantId,
    ProcessedImageSet files,
    CancellationToken ct = default);
```

---

## 8. Frontend (Angular)

### 8.1 Pantalla

**Inventario → «Importar fotos ZIP»** (menú o toolbar lista)

Componentes:

- `variant-image-import-dialog` — upload + flags
- `variant-image-import-progress` — polling batch
- `variant-image-import-report` — tabla logs exportable CSV

### 8.2 Servicio

```typescript
importZip(file: File, options: { replaceExisting: boolean; useAiProcessing: boolean }): Observable<{ batchId: string }>
getBatch(batchId: string): Observable<ImageImportBatchDto>
getBatchLogs(batchId: string, page: number): Observable<ImageImportLogDto[]>
```

### 8.3 UX copy

- «Nombrá las fotos `{SKU}_1`, `{SKU}_2`, `{SKU}_3`. El SKU debe coincidir con una variante de tu negocio.»
- Link a doc / ejemplo ZIP plantilla descargable (fase 1.1)

---

## 9. Plan de implementación

### Sprint A — SKU único (bloqueante, ~3–5 días)

| # | Tarea |
|---|--------|
| A1 | Script detectar duplicados por negocio |
| A2 | Migración `SkuNormalized` + índice único `(BusinessId, SkuNormalized)` |
| A3 | `IVariantRepository.FindByBusinessAndSkuAsync` + validación create/update |
| A4 | Tests unitarios + mensajes ES |
| A5 | FE: error claro al duplicar SKU |

### Sprint B — Import MVP sin IA (~5–8 días)

| # | Tarea |
|---|--------|
| B1 | Entidades batch/log + migración |
| B2 | `LocalImageSharpProductImageEnhancerService` + tests snapshot |
| B3 | `ImportVariantImagesZipUseCase` + background worker |
| B4 | Controller + límites configurables |
| B5 | Extender `CatalogVariantImage` URLs + storage multi-size |
| B6 | FE modal import + reporte |
| B7 | Actualizar DTOs listado (`PrimaryImageUrl` → thumb) |

### Sprint C — IA opcional (~3–5 días)

| # | Tarea |
|---|--------|
| C1 | Integrar Cloudinary **o** Clipdrop |
| C2 | Flag `useAiProcessing` |
| C3 | Métricas costo/tiempo en log |

---

## 10. Smoke test end-to-end

- [ ] Migración SKU: duplicados bloqueados
- [ ] Crear variantes `LAPTOP-HP840G5`, `MOUSE-LOGITECH-M185` con SKUs únicos
- [ ] ZIP con 4 archivos → batch 202 → polling hasta Completed
- [ ] Imágenes 1200/600/300 WebP en Supabase
- [ ] `_1` marcada `IsPrimary`
- [ ] Segundo import sin `replaceExisting` → SkippedExisting
- [ ] Con `replaceExisting=true` → reemplaza + borra objetos viejos
- [ ] SKU inexistente en ZIP → log error, resto continúa
- [ ] Tienda pública muestra thumbs uniformes

---

## 11. Riesgos y mitigaciones

| Riesgo | Mitigación |
|--------|------------|
| Duplicados SKU en prod | Script pre-migración + limpieza Joyca |
| Timeout Railway | Siempre async batch |
| IA altera producto | Fallback local; revisión manual muestra |
| Costo IA | `useAiProcessing=false` default en MVP |
| ZIP malicioso | Validar paths, límite entries, scan magic bytes |

---

## 12. Referencias código actual

| Pieza | Ubicación |
|-------|-----------|
| Upload manual | `UploadCatalogVariantImagesUseCase`, `VariantController` |
| Storage | `SupabaseVariantImageStorageService` |
| ImageSharp | `CampaignImageProcessor` |
| Background job | `CampaignQueueBackgroundService` |
| FE imágenes | `variant-item-images/*`, `variants.service.ts` |
| SKU hoy | `VariantRepository.ExistsSkuForCatalogItemAsync` |

---

*Última actualización: 13 junio 2026*
