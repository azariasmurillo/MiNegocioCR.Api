# App móvil Flutter — Inventario rápido (diseño v2)

> **Spec unificada** (v1 + mejoras estratégicas). Para desarrollo de la app Android y extensiones API.  
> Complementa [VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md](./VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md), [INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md) y reemplaza [MOBILE_INVENTARIO_FLUTTER_v1.md](./MOBILE_INVENTARIO_FLUTTER_v1.md) como referencia activa.

| Decisión | Valor |
|----------|--------|
| Plataforma inicial | **Android** (Flutter) |
| Distribución | Descarga APK desde panel MiNegocioCR |
| Auth | Mismo JWT que la web (`POST /api/auth/login`) |
| Escaneo MVP | Código escaneado → lookup por SKU (`barcode = sku` por convención) |
| Escaneo M1+ | Campo **`Barcode`** separado de **`SKU`** en `CatalogVariant` |
| Fotos | Máx. **3** por variante, enhancer marketplace WebP |
| IA | **Fase B** — Gemini Flash vision; **nunca persiste solo** |
| Recorte fondo IA | **Fase C** — Sprint C API (`useBackgroundRemoval`) |

**Estado código (jun 2026):**
- [x] `GET /api/variants/by-sku/{sku}` — lookup exacto por tenant
- [ ] Barcode separado, endpoints móvil, Flutter app

---

## 0. Objetivo estratégico

Mantener el **MVP simple y rápido**, pero diseñar arquitectura desde hoy para:

- Inventario masivo en piso (modo caja)
- IA asistida sin contaminar el catálogo
- Catálogos escalables (presentaciones, no productos duplicados)
- Compatibilidad futura (inventario visual por cámara — sin implementar aún)

### Regla de oro (norma de producto)

**La IA nunca debe:**

- Crear productos automáticamente
- Crear variantes / presentaciones automáticamente
- Crear dimensiones automáticamente

**La IA únicamente puede:**

- Sugerir, clasificar, detectar coincidencias, extraer información (OCR/atributos)

**La persistencia siempre requiere confirmación humana** (`quick-create`, `adjust`, upload explícito).

---

## 1. Visión funcional

App en el celular del comerciante para:

1. **Escanear código de barras** → resolver variante en su negocio.
2. **Si existe:** agregar stock (modo caja), actualizar fotos, ver historial reciente.
3. **Si no existe:** capturar hasta 3 fotos, completar datos, crear producto simple.
4. **(Fase B)** IA sugiere nombre, categoría, **presentación vs producto nuevo**; usuario confirma en pantalla de revisión.
5. **(Fase B)** Borrador intermedio `PendingProduct` antes de tocar `CatalogItem`.

---

## 2. Modelo de datos actual vs evolución

### Hoy (`CatalogVariant`)

```csharp
public string? SKU;
public string? SkuNormalized;  // índice único por BusinessId
// No existe Barcode
```

Lookup implementado: `GET /api/variants/by-sku/{sku}` → `GetVariantBySkuUseCase`.

### M1 — Separar Barcode de SKU (recomendado antes de escala)

**Problema:** muchos negocios usan SKU interno distinto al EAN de fábrica.

```text
SKU interno:     JOY-HP-65W
Barcode (EAN):   7501234567890
```

**Propuesta:**

```csharp
public class CatalogVariant
{
    public string? SKU;
    public string? SkuNormalized;

    public string? Barcode;              // EAN/UPC fabricante
    public string? BarcodeNormalized;    // LOWER(TRIM), índice único nullable por negocio
}
```

| Regla | Detalle |
|-------|---------|
| Unicidad | `(BusinessId, SkuNormalized)` y `(BusinessId, BarcodeNormalized)` — parciales, nullable |
| MVP Flutter | Al crear desde escaneo: `SKU = Barcode = código escaneado` hasta que el usuario edite |
| Import ZIP | Sigue matcheando por **SKU** (nombres `{SKU}_1.jpg`) |
| Lookup unificado | `GET /api/mobile/inventory/scan?code={code}` → busca barcode, luego sku |

**Beneficios:** compatible con fabricantes, supermercados y catálogos grandes; evita migración dolorosa después.

---

## 3. Fases de entrega (prioridad cerrada)

### M0 — Fundación API ✅

- [x] `GET /api/variants/by-sku/{sku}`
- [x] Spec v1/v2

### Fase A — APK usable (sin IA)

**Flutter:**

- [ ] Login JWT (`POST /api/auth/login`, `flutter_secure_storage`)
- [ ] Escáner (`mobile_scanner`) → lookup
- [ ] Flujo variante encontrada: stock + fotos
- [ ] Flujo producto nuevo: wizard simple (1 presentación)
- [ ] **Modo caja** — ingreso masivo (ver §6)
- [ ] **Historial local** — actividad reciente (ver §5)
- [ ] Descarga APK desde web MiNegocioCR

**API (opcional pero recomendado):**

- [ ] `POST /api/mobile/inventory/quick-create` — catalog + variant + fotos en transacción
- [ ] `POST /api/mobile/inventory/{variantId}/photos` — enhancer `marketplace-white-v1`

### M1 — Barcode separado (API, antes de muchos clientes con EAN real)

- [ ] Migración `Barcode` + `BarcodeNormalized` en `CatalogVariants`
- [ ] `GET /api/variants/by-barcode/{barcode}` o `GET /api/mobile/inventory/scan`
- [ ] Create/Update variant acepta `barcode` opcional
- [ ] Web inventario: campo barcode en edición variante (opcional)

### Fase B — IA asistida (Gemini)

- [ ] `POST /api/mobile/inventory/ai-suggest` — fotos → JSON sugerencia (**sin persistir**)
- [ ] Entidad **`PendingProduct`** / `MobileProductDraft` con TTL (ver §4)
- [ ] **Prioridad IA:** sugerir **nueva presentación** antes que producto nuevo (ver §7)
- [ ] OCR / atributos del empaque vía Gemini Vision (marca, modelo, capacidad, color)
- [ ] Detección de duplicados fuzzy (nombre/marca/categoría similares)
- [ ] Pantalla revisión humana obligatoria
- [ ] Rate limit por negocio (ej. 50 sugerencias/día plan básico)

### Fase C — Catálogo avanzado

- [ ] Recorte de fondo real (`useBackgroundRemoval` — Sprint C API)
- [ ] Matching avanzado catálogo (embeddings / scores de similitud)
- [ ] **Inventario visual** — video → IA → conteo (solo diseño; sin código aún)
- [ ] Cola offline en app (opcional)
- [ ] iOS (opcional)

---

## 4. PendingProduct (Fase B)

### Problema

La IA puede equivocarse. No queremos contaminar `CatalogItem` / `CatalogVariant` hasta confirmación.

### Flujo

```text
Fotos (+ barcode opcional)
    ↓
POST /api/mobile/inventory/ai-suggest  (stateless o crea draft)
    ↓
PendingProduct (BD o draft id)
    ↓
Usuario revisa / edita en app
    ↓
POST /api/mobile/inventory/quick-create  (confirma)
    ↓
CatalogItem + CatalogVariant definitivos
```

### Entidad propuesta (borrador)

```csharp
public class MobileProductDraft
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? ScannedCode { get; set; }
    public string Status { get; set; }  // Draft | Confirmed | Discarded
    public string AiSuggestionJson { get; set; }  // snapshot respuesta IA
    public string? UserOverridesJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }  // ej. +7 días
}
```

Fotos del draft: staging Supabase o paths temporales; mover a variante solo al confirmar.

**Alternativa MVP B:** draft solo en SQLite local en Flutter (más rápido, sin multi-dispositivo).

---

## 5. Historial de escaneos (Fase A — alto valor, bajo costo)

### Problema

En carga masiva el usuario puede equivocarse y no recordar qué escaneó.

### Pantalla «Actividad reciente»

```text
✓ Mouse Logitech M185     (+5)   hace 2 min
✓ Cargador HP 65W         (+3)   hace 5 min
✗ SKU 750123…             no encontrado → creado
```

### Implementación

| Fase | Dónde |
|------|--------|
| **A** | SQLite / Hive en Flutter — `{ at, code, variantId?, action, qty, result }` |
| **B+** | `MobileScanEvent` en API — auditoría multi-usuario |

No reemplaza `InventoryMovement`; es UX + corrección rápida.

---

## 6. Modo caja (Fase A — incluir en MVP)

Flujo optimizado para recepción de mercadería:

```text
[Escanear] → producto encontrado → [Cantidad: 24] → [Guardar] → listo para siguiente escaneo
```

Sin repetir navegación. Backend: solo `POST /api/inventory/adjust` en loop.

```json
{
  "businessId": "uuid",
  "variantId": "uuid",
  "adjustment": 24,
  "reason": "Compra / ingreso móvil"
}
```

Opcional: atajo «+1» para conteo uno a uno.

---

## 7. IA: presentación antes que producto (Fase B — regla #1)

### Problema

Sin esta regla, la IA crea duplicados:

```text
❌ Coca Cola 600 ml   (CatalogItem)
❌ Coca Cola 1.5 L    (CatalogItem)
❌ Coca Cola 3 L      (CatalogItem)
```

### Modelo correcto (ya existe en MiNegocioCR)

```text
✅ CatalogItem: "Coca Cola"
   ├── Presentación: 600 ml
   ├── Presentación: 1.5 L
   └── Presentación: 3 L
```

### Regla para prompt Gemini

Antes de sugerir `new-product`:

1. Buscar coincidencias por nombre base / marca
2. Buscar mismo `CatalogItem` en categoría similar
3. Buscar dimensiones ya usadas (Capacidad, Marca, Color…)

**Respuesta preferida:**

```json
{
  "action": "new-presentation",
  "catalogItemId": "uuid",
  "catalogItemName": "Coca Cola",
  "dimensionName": "Capacidad",
  "dimensionValue": "600 ml",
  "confidence": 0.91,
  "alternativeAction": "new-product",
  "warnings": []
}
```

Solo si no hay match razonable → `"action": "new-product"`.

---

## 8. OCR / atributos del empaque (Fase B)

No requiere pipeline OCR separado al inicio — **Gemini Vision** extrae texto y estructura.

Foto del empaque:

```text
HP
USB-C
65W
```

Salida sugerida:

```json
{
  "brand": "HP",
  "power": "65W",
  "connector": "USB-C",
  "suggestedProductName": "Cargador HP USB-C 65W"
}
```

Complementa barcode cuando el producto no existe o el código no está en catálogo.

---

## 9. Detección de duplicados (Fase B)

Antes de `new-product`, ejecutar:

| Señal | Método MVP | Método B |
|-------|------------|----------|
| Código exacto | `by-sku` / `by-barcode` | igual |
| Nombre similar | `GET /variants/business?search=` | + score Gemini |
| Marca / categoría | prompt IA + catálogo en contexto | embeddings |

UI:

```text
Encontramos un producto muy parecido: «Cargador HP USB-C 60W».
¿Desea agregar una presentación o usar el existente?
[ Usar existente ]  [ Crear igual ]  [ Cancelar ]
```

Evitar falsos positivos agresivos (umbral ≥ 0.85 y confirmación explícita).

---

## 10. Inventario visual (Fase C — solo visión)

No implementar ahora. Diseñar APIs pensando en:

```text
Video / burst de fotos
    ↓
Job async (patrón ImageImportBatch)
    ↓
IA detecta productos + conteo
    ↓
Usuario confirma ajustes de stock
```

Reutilizar: workers en background, lotes, logs por ítem.

---

## 11. Backend reutilizable hoy

| Capa | Endpoint / componente | Fase |
|------|----------------------|------|
| Login | `POST /api/auth/login` | A |
| Sesión | `GET /api/auth/me` | A |
| Config | `GET /api/businesses/{id}/config` | A |
| Lookup SKU | `GET /api/variants/by-sku/{sku}` ✅ | A |
| Lookup barcode | `GET /api/variants/by-barcode/{barcode}` | M1 |
| Scan unificado | `GET /api/mobile/inventory/scan?code=` | M1 |
| Listado | `GET /api/variants/business/{id}?search=` | A/B |
| Crear | `POST /api/catalog` + `POST /api/variants` | A |
| Stock | `POST /api/inventory/adjust` | A |
| Fotos | `POST /api/variants/{id}/images` | A |
| Enhancer | ImageSharp + WebP 3 tamaños | A |
| IA suggest | `POST /api/mobile/inventory/ai-suggest` | B |
| Quick create | `POST /api/mobile/inventory/quick-create` | A |
| Draft | `MobileProductDraft` CRUD | B |

---

## 12. API implementada — `GET /api/variants/by-sku/{sku}`

**Auth:** Bearer JWT; `businessId` del token.

**200** — `VariantBySkuLookupDto`:

```json
{
  "variantId": "uuid",
  "catalogItemId": "uuid",
  "catalogItemName": "Mouse Logitech M185",
  "presentationLabel": "Negro",
  "sku": "1234567890123",
  "currentStock": 5,
  "price": 8500,
  "costPrice": 4500,
  "profitMargin": 50,
  "effectiveProfitMargin": 50,
  "primaryImageUrl": "https://...",
  "imageCount": 2,
  "isActive": true,
  "optionValueLabels": ["Color: Negro"]
}
```

**404** — no existe variante con ese SKU en el negocio.

---

## 13. API propuesta — `ai-suggest` (Fase B)

**POST** `/api/mobile/inventory/ai-suggest`  
Multipart: 1–3 imágenes + `scannedCode?` + `businessId` (del token)

```json
{
  "confidence": 0.82,
  "action": "new-presentation",
  "suggestedProductName": "Cargador HP USB-C 65W",
  "suggestedCategoryId": "uuid-or-null",
  "matchExisting": {
    "catalogItemId": "uuid",
    "catalogItemName": "Cargadores USB-C",
    "reason": "Misma familia de producto"
  },
  "suggestedPresentation": {
    "dimensionName": "Potencia",
    "valueText": "65W"
  },
  "extractedAttributes": {
    "brand": "HP",
    "connector": "USB-C",
    "power": "65W"
  },
  "duplicateCandidates": [
    { "catalogItemId": "uuid", "similarity": 0.88, "name": "Cargador HP 60W" }
  ],
  "warnings": []
}
```

---

## 14. Flujos UX resumidos

### 14.1 Login

```text
Email + contraseña → JWT → flutter_secure_storage
Header: Authorization: Bearer {token}
```

### 14.2 Escaneo normal

```text
Scan → GET by-sku (M1: scan unificado)
  200 → VariantFoundScreen
  404 → ProductNotFoundScreen → fotos + formulario
```

### 14.3 Modo caja

```text
Scan → found → cantidad → adjust → historial local → scan siguiente
```

### 14.4 Producto nuevo con IA (Fase B)

```text
Scan 404 → fotos → ai-suggest → PendingReviewScreen → confirm → quick-create
```

### 14.5 Precio

| Modo | API |
|------|-----|
| Fijo | `setPriceManually: true` |
| Calculado | costo + margen % + IVA (`CatalogVariantPriceResolver`) |

Default: `GET /api/businesses/{id}/config` → `defaultProfitMargin`.

---

## 15. Flutter — stack y pantallas

| Paquete | Uso |
|---------|-----|
| `dio` | HTTP + JWT interceptor |
| `flutter_secure_storage` | Token |
| `mobile_scanner` | Barcode |
| `image_picker` | Cámara |
| `riverpod` | Estado |
| `sqflite` / `hive` | Historial local (Fase A) |

### Pantallas

| # | Pantalla | Fase |
|---|----------|------|
| 1 | LoginScreen | A |
| 2 | HomeScreen (Escanear / Modo caja / Actividad) | A |
| 3 | BarcodeScannerScreen | A |
| 4 | VariantFoundScreen | A |
| 5 | BoxModeQuantityScreen | A |
| 6 | ProductNotFoundScreen | A |
| 7 | PhotoCaptureScreen | A |
| 8 | ProductFormScreen | A |
| 9 | RecentActivityScreen | A |
| 10 | PhotoPreviewEnhancedScreen | A |
| 11 | AiSuggestionReviewScreen | B |
| 12 | PendingProductReviewScreen | B |
| 13 | SuccessScreen | A |

---

## 16. Imágenes marketplace

Reutilizar pipeline existente:

- Estilos: `marketplace-white-v1` (default), `marketplace-soft-v1`
- Salida: WebP 1200 / 600 / 300 px
- Storage: Supabase `business-assets`
- Fase C: `useBackgroundRemoval`

---

## 17. Seguridad

- JWT + `businessId` en token; lookups acotados al tenant
- IA: rate limit por negocio; no persistir sugerencias como catálogo
- Fotos: máx 5 MB, png/jpeg
- Drafts: TTL y borrado al confirmar/descartar
- MVP online-only

---

## 18. Preguntas abiertas (producto)

| # | Pregunta | Default propuesto |
|---|----------|-------------------|
| 1 | ¿Barcode = SKU en MVP? | Sí, hasta M1 |
| 2 | ¿Multidimensional desde celular? | No en A; solo presentación simple. B: IA sugiere dimensión |
| 3 | ¿Costo IA incluido en plan? | Límite diario + upgrade |
| 4 | ¿Solo inventario o también consulta precio POS? | A: inventario; consulta precio = fase futura |
| 5 | ¿iOS? | Después de Android estable |

---

## 19. Prompt para Gemini — Fase A Flutter

```text
Implementá la Fase A de «MiNegocioCR Inventario Rápido» (Android, Flutter 3).

API base: MiNegocioCR.Api (JWT Bearer).

Funcionalidad:
1. Login POST /api/auth/login → flutter_secure_storage
2. Escáner mobile_scanner → GET /api/variants/by-sku/{code}
3. Si 200: pantalla variante + POST /api/inventory/adjust + upload fotos (máx 3)
4. Si 404: wizard crear POST /api/catalog + POST /api/variants + fotos
5. Modo caja: escaneo repetido + cantidad + adjust sin salir del flujo
6. Historial local SQLite: últimos 50 escaneos
7. Precio fijo o calculado; margen desde GET /api/businesses/{id}/config
8. SIN IA en Fase A

Stack: Riverpod, dio, mobile_scanner, image_picker, flutter_secure_storage, sqflite.

Regla: no inventar endpoints; usar los documentados en MOBILE_INVENTARIO_FLUTTER_v2.md.
```

---

## 20. Prompt para Gemini — Fase B IA

```text
Extendé la app con Fase B según MOBILE_INVENTARIO_FLUTTER_v2.md.

Backend: POST /api/mobile/inventory/ai-suggest (Gemini 2.0 Flash vision).

Reglas IA (obligatorias):
- NUNCA persistir catálogo automáticamente
- Priorizar action "new-presentation" sobre "new-product"
- Incluir extractedAttributes (marca, capacidad, color…) del empaque
- Mostrar duplicateCandidates si similarity > 0.85
- Usuario confirma en PendingProductReviewScreen → POST quick-create

PendingProduct: draft local (SQLite) o MobileProductDraft en API si existe.

Rate limit: manejar 429 con mensaje amigable.
```

---

## 21. Referencias de código

| Tema | Ubicación |
|------|-----------|
| Lookup SKU | `GetVariantBySkuUseCase`, `VariantController.GetBySku` |
| SKU normalizado | `SkuNormalizer`, migración `AddUniqueSkuPerBusiness` |
| Enhancer | `LocalImageSharpProductImageEnhancerService` |
| Import async | `ImageImportBatchProcessor`, `ImageImportBackgroundService` |
| Orquestación web | `inventory-orchestrator.service.ts` |
| Presentaciones | `INVENTARIO_UX_REDISENO_v2.md` §5 |

---

## 22. Changelog v1 → v2

| Tema | v1 | v2 |
|------|----|----|
| Barcode ≠ SKU | Fase C | **M1 API** (antes de escala) |
| Modo caja | — | **Fase A** |
| Historial escaneos | — | **Fase A** (local) |
| PendingProduct | implícito | **entidad / draft Fase B** |
| IA presentación vs producto | mencionado | **regla #1 del prompt** |
| OCR | — | **Gemini Vision Fase B** |
| Duplicados fuzzy | — | **Fase B** |
| Inventario visual | — | **Fase C visión** |
| Regla IA no persiste | sí | **norma de producto §0** |
