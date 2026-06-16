# App móvil Flutter — Inventario rápido (diseño v2)

> **Spec unificada** (v1 + mejoras estratégicas). Para desarrollo de la app Android y extensiones API.  
> Complementa [VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md](./VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md), [INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md) y reemplaza [MOBILE_INVENTARIO_FLUTTER_v1.md](./MOBILE_INVENTARIO_FLUTTER_v1.md) como referencia activa.

| Decisión | Valor |
|----------|--------|
| Plataforma inicial | **Android** (Flutter) |
| Distribución | **Descarga APK desde MiNegocioCR web** (ver §23.0) — hoy **no implementado** |
| Auth | Mismo JWT que la web (`POST /api/auth/login`) |
| Escaneo MVP | Código escaneado → lookup por SKU (`barcode = sku` por convención) |
| Escaneo M1+ | Campo **`Barcode`** separado de **`SKU`** en `CatalogVariant` |
| Fotos | Máx. **3** por variante; **ImageSharp** estilo Amazon (`marketplace-white-v1`) — ver §16 |
| IA (Gemini) | **Fase B** — solo **lee** fotos para clasificar; **no modifica imágenes** |
| Mejora visual fotos | **ImageSharp** (API existente), **no Gemini** — decisión cerrada §16 |
| Recorte fondo IA | **Fuera de alcance móvil** por ahora; opcional futuro ZIP API (Sprint C), no requerido |

**Estado código (jun 2026):**
- [x] `GET /api/variants/by-sku/{sku}` — lookup exacto por tenant
- [ ] Distribución APK en web, barcode separado, endpoints móvil, Flutter app

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

- Sugerir, clasificar, detectar coincidencias, extraer información (OCR/atributos) **desde** fotos
- **No** retocar, recortar, cambiar fondo ni generar archivos de imagen

**La persistencia siempre requiere confirmación humana** (`quick-create`, `adjust`, upload explícito).

### Decisión cerrada — fotos vs Gemini (jun 2026)

| Responsable | Qué hace | Qué **no** hace |
|-------------|----------|-----------------|
| **ImageSharp** (`LocalImageSharpProductImageEnhancerService`) | Fondo blanco tipo Amazon, resize, WebP 3 tamaños, sombra contacto | Clasificar producto |
| **Gemini (Fase B)** | Leer foto → JSON (nombre, categoría, marca, presentación sugerida) | Retocar, recortar, cambiar fondo ni generar imagen |
| **Usuario** | Confirma sugerencias y ve preview **ya mejorada por ImageSharp** antes de guardar | — |

El estilo visual actual **marketplace-white-v1** se mantiene; no se planea que Gemini procese fotos en la app móvil.

### Regla UX estratégica (§23.7)

**90% de las entradas de inventario** deben completarse en **menos de 15 segundos** desde el escaneo. Toda pantalla o campo extra debe justificarse contra esta regla.

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
- [ ] **Pantalla resumen** antes de crear producto nuevo (ver §23.3)
- [ ] **Modo caja:** cantidad + costo opcional + razón (ver §23.1–23.2)

**Web MiNegocioCR (distribución APK — §23.0):**

- [ ] Pantalla / sección «App móvil inventario» con botón descargar APK
- [ ] `GET /api/mobile/app/release` — versión, URL, notas, tamaño
- [ ] APK alojada en Supabase Storage o CDN (CI sube artefacto)

**API (recomendado para Fase A):**

- [ ] `POST /api/mobile/inventory/quick-create` — catalog + variant + fotos en transacción
- [ ] `POST /api/mobile/inventory/{variantId}/photos` — enhancer `marketplace-white-v1`
- [ ] `POST /api/mobile/inventory/receive` — stock + costo opcional + razón (ver §23.1) — extiende `adjust`

### M1 — Barcode separado + auditoría móvil (API)

- [ ] Migración `Barcode` + `BarcodeNormalized` en `CatalogVariants`
- [ ] `GET /api/variants/by-barcode/{barcode}` o `GET /api/mobile/inventory/scan`
- [ ] Create/Update variant acepta `barcode` opcional
- [ ] Web inventario: campo barcode en edición variante (opcional)
- [ ] `InventoryMovement`: `UserId`, `StockBefore`, `StockAfter`, `ReasonCode` (ver §23.2, §23.5)
- [ ] `CatalogVariantImage.IsPublic` default `true` (ver §23.4)

### Fase B — IA asistida (Gemini) — solo datos, no imágenes

- [ ] `POST /api/mobile/inventory/ai-suggest` — envía fotos **solo para análisis**; respuesta **JSON** (**sin** imagen procesada)
- [ ] Entidad **`PendingProduct`** / `MobileProductDraft` con TTL (ver §4)
- [ ] **Prioridad IA:** sugerir **nueva presentación** antes que producto nuevo (ver §7)
- [ ] OCR / atributos del empaque vía Gemini Vision (marca, modelo, capacidad, color)
- [ ] Detección de duplicados fuzzy (nombre/marca/categoría similares)
- [ ] **Aprendizaje por negocio** en prompt: top marcas, categorías, productos recientes (§23.6)
- [ ] Pantalla revisión humana obligatoria
- [ ] Rate limit por negocio (ej. 50 sugerencias/día plan básico)

### Fase C — Catálogo avanzado (opcional / futuro)

- [ ] Matching avanzado catálogo (embeddings / scores de similitud)
- [ ] **Inventario visual** — video → IA → conteo (solo diseño; sin código aún)
- [ ] Cola offline en app (opcional)
- [ ] iOS (opcional)
- [ ] *(Opcional, no móvil)* Recorte fondo `useBackgroundRemoval` en import ZIP — ver [VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md](./VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md) Sprint C; **no prioritario**; ImageSharp Amazon-style es suficiente

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

Gemini **analiza** la foto (vision) y devuelve **texto/JSON únicamente**. La imagen que se guarda en inventario pasa **después** por ImageSharp (§16), no por Gemini.

No requiere pipeline OCR separado — **Gemini Vision** extrae texto y estructura.

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
  404 → ProductNotFoundScreen → fotos + formulario → ProductSummaryScreen → confirm
```

### 14.3 Modo caja

```text
Scan → found → cantidad → [costo unitario opcional] → [razón: Compra] → receive/adjust → historial → scan siguiente
```

**Objetivo:** ≤ 15 s por ítem cuando costo no se captura (default más común).

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
| 9 | **ProductSummaryScreen** (confirmar antes de guardar) | A |
| 10 | RecentActivityScreen | A |
| 11 | PhotoPreviewEnhancedScreen | A |
| 12 | **MobileAppDownload** (web: Configuración o Inventario) | A |
| 13 | AiSuggestionReviewScreen | B |
| 14 | PendingProductReviewScreen | B |
| 15 | SuccessScreen | A |

---

## 16. Imágenes marketplace — pipeline de fotos (decisión cerrada)

### Principio

**Toda foto que entra al inventario (móvil, web o ZIP) se mejora con ImageSharp, estilo Amazon.**  
**Gemini no participa en este pipeline** — solo ayuda a llenar datos del producto en Fase B.

### Flujo móvil (Fase A)

```text
Usuario toma foto (JPEG/PNG)
        ↓
POST /api/mobile/inventory/{variantId}/photos
  (o quick-create con adjuntos)
        ↓
LocalImageSharpProductImageEnhancerService
  • marketplace-white-v1 (default) — fondo #FFFFFF, producto centrado, sombra contacto
  • marketplace-soft-v1 (opcional)
  • WebP: 1200 / 600 / 300 px
        ↓
Supabase business-assets
        ↓
App muestra PhotoPreviewEnhancedScreen (preview URLs devueltas)
        ↓
Usuario confirma → variante queda con fotos marketplace
```

### Qué hace ImageSharp (ya implementado en import ZIP)

| Paso | Detalle |
|------|---------|
| Canvas | Blanco tipo Amazon (`marketplace-white-v1`) |
| Producto | Centrado, ratio ~85% del canvas |
| Sombra | Contact shadow bajo el producto |
| Salida | WebP calidad ~88%, 3 tamaños |
| Referencia | `LocalImageSharpProductImageEnhancerService`, spec [VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md](./VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md) |

### Qué hace Gemini (Fase B) — explícitamente NO

| Prohibido | Motivo |
|-----------|--------|
| Devolver imagen retocada | Costo, latencia, inconsistencia visual |
| Quitar fondo con IA | Fuera de alcance; ImageSharp alcanza para MVP |
| Generar escenas / fondos creativos | No es marketplace |

Gemini recibe la misma foto **solo como input multimodal** para `ai-suggest` → JSON.

### Estilos disponibles

- **`marketplace-white-v1`** — default app móvil (Amazon-style) ✅
- **`marketplace-soft-v1`** — gris suave (opcional en settings)

### Fuera de alcance móvil (por ahora)

- `useBackgroundRemoval` / Sprint C del import ZIP — **no requerido** para app móvil; evaluar solo si fotos con fondo muy sucio lo exigen en prod.

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
9. La APK se descarga desde MiNegocioCR web (GET /api/mobile/app/release), no desde la app — ver §23.0

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
- NUNCA procesar, retocar ni devolver imágenes — solo JSON de sugerencia
- Las fotos finales las mejora ImageSharp (marketplace-white-v1) en el API, igual que import ZIP
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

---

## 23. Ajustes recomendados antes de implementar

> Incorporados desde revisión externa (Chat). Alto valor, bajo costo de diseño; evitan refactor después.

### 23.0 Distribución APK desde MiNegocioCR web ⚠️ pendiente

**Problema:** hoy la spec menciona «descargar APK desde el panel» pero **no hay diseño ni pantalla** en web ni endpoint de release.

**Objetivo:** el usuario autenticado descarga la app desde MiNegocioCR cuando la necesite, sin Play Store (MVP).

#### UX web (Fase A)

Ubicación sugerida (elegir una o ambas):

| Ubicación | Ventaja |
|-----------|---------|
| **Configuración del negocio** → pestaña «App móvil» | Descubrible para admins |
| **Inventario** → banner / menú «Descargar app de escaneo» | Contexto de uso |

Contenido de la pantalla:

```text
App Inventario Rápido (Android)
Versión: 1.0.0 (build 42)
Tamaño: ~18 MB

[ Descargar APK ]
[ Ver instrucciones de instalación ]

QR → misma URL (opcional)
```

Instrucciones mínimas: habilitar «orígenes desconocidos», instalar, iniciar sesión con la misma cuenta web.

#### API propuesta

**GET** `/api/mobile/app/release` — `[Authorize]`, cualquier usuario del negocio

```json
{
  "platform": "android",
  "versionName": "1.0.0",
  "versionCode": 42,
  "downloadUrl": "https://...supabase.../releases/minnegociocr-inventario-1.0.0.apk",
  "releaseNotes": "Modo caja, escáner, fotos marketplace.",
  "fileSizeBytes": 18874368,
  "minApiLevel": 24,
  "publishedAt": "2026-06-15T00:00:00Z"
}
```

| Decisión | Valor MVP |
|----------|-----------|
| Storage | Supabase bucket `platform-releases` o path fijo en CDN |
| Upload | CI (GitHub Actions) al merge tag `mobile-v*` |
| Multi-tenant | **Una APK global** para todos los negocios (mismo backend) |
| iOS | Futuro — endpoint puede devolver `platform: ios` + TestFlight URL |

#### Frontend (Angular)

- Ruta: `/settings/mobile-app` o componente en `business-settings`
- Servicio: `MobileAppReleaseService` → `GET /api/mobile/app/release`
- Botón: `<a href="downloadUrl" download>` o redirect

#### Checklist

- [ ] Bucket Supabase + política lectura pública del APK (o signed URL 24h)
- [ ] Endpoint API + config `MobileAppReleaseOptions` en appsettings
- [ ] Pantalla web con descarga
- [ ] Pipeline CI que publique APK al bucket

---

### 23.1 Captura de costo durante ingreso de inventario

**Problema hoy:** `POST /api/inventory/adjust` solo mueve stock (`AdjustInventoryUseCase` → `InventoryService.AdjustStockAsync`). **No actualiza `CostPrice`.**

**Realidad comercial:** llega mercadería nueva y cambia el costo de compra.

```text
Mouse Logitech — stock 5, costo ₡4.500
Nueva compra: +10 unidades a ₡5.000
```

#### Flujo modo caja (propuesto)

```text
Escanear → encontrado → Cantidad → Costo unitario (opcional) → Guardar
```

| Si usuario… | Sistema… |
|-------------|----------|
| No indica costo | Solo stock (+ movimiento) |
| Indica costo | Stock + actualizar costo según política del negocio |

#### Política de costo (config negocio — futuro)

| Modo | Fórmula |
|------|---------|
| `replace` | Nuevo costo reemplaza el anterior |
| `weighted_average` | `(stock_viejo × costo_viejo + qty × costo_nuevo) / stock_nuevo` |

**MVP sugerido:** `replace` (más simple) o solo guardar costo en el movimiento sin recalcular hasta M1.

#### API propuesta

**POST** `/api/mobile/inventory/receive`

```json
{
  "variantId": "uuid",
  "quantity": 10,
  "unitCost": 5000,
  "reasonCode": "purchase",
  "reasonNote": "Factura #1234"
}
```

Extiende o reemplaza adjust en app móvil; web puede seguir con `adjust` hasta unificar.

**Gap vs código actual:** requiere extender `AdjustInventoryRequestDto` o nuevo use case + opcional recalcular precio si modo calculado.

---

### 23.2 Razón del movimiento de inventario

**Hoy:** `AdjustInventoryRequestDto.Reason` es **string libre** guardado en `InventoryMovement.Notes`. No hay catálogo ni `ReasonCode`.

**Propuesta:** enum / código + nota opcional.

| `reasonCode` | Etiqueta ES |
|--------------|-------------|
| `purchase` | Compra |
| `initial_stock` | Inventario inicial |
| `manual_adjustment` | Ajuste manual |
| `correction` | Corrección |
| `supplier_return` | Devolución proveedor |
| `customer_return` | Devolución cliente |
| `production` | Producción |
| `other` | Otro |

App móvil: dropdown con default **`purchase`** en modo caja (1 tap). Nota libre colapsada.

**Beneficios:** kardex, reportes, auditoría.

---

### 23.3 Pantalla de revisión final (producto nuevo)

**Problema:** errores al crear producto sin chance de revisar.

```text
Formulario → Resumen → Confirmar → Guardar
```

Mostrar:

```text
Nombre, Categoría, SKU, Barcode, Costo, Precio, Stock inicial, Fotos (preview)
```

**Fase A Flutter** — `ProductSummaryScreen`. Cumple regla §23.7 si el resumen es una sola pantalla de confirmación (no formulario extra largo).

---

### 23.4 Fotos públicas vs fotos internas

**Hoy:** `CatalogVariantImage` **no tiene** `IsPublic`. Todas las URLs van al marketplace si se publican.

**Propuesta (migración M1):**

```csharp
public class CatalogVariantImage
{
    // ... existente ...
    public bool IsPublic { get; set; } = true;
}
```

| MVP | Todas `IsPublic = true` |
| Futuro | Fotos internas: factura, caja, serie, daños — no en tienda digital |

Marketplace / tienda digital: filtrar `IsPublic == true`.

---

### 23.5 Auditoría mínima desde Fase A

**Historial local (app)** ayuda al usuario; **no sustituye** auditoría en servidor.

Cada movimiento de inventario debería registrar (migración M1):

| Campo | Descripción |
|-------|-------------|
| `UserId` | Quién hizo el ajuste (JWT) |
| `CreatedAt` | Cuándo |
| `Quantity` | Delta (+/-) |
| `ReasonCode` / `Notes` | Por qué |
| `StockBefore` | Stock antes |
| `StockAfter` | Stock después |
| `Source` | `web` \| `mobile` \| `pos` |

**Hoy:** `InventoryMovement` tiene `Quantity`, `Notes`, `Type`, `CreatedAt` — **sin** `UserId`, stock before/after, reason code.

**Beneficio:** investigar diferencias, kardex futuro, cumplimiento.

---

### 23.6 Aprendizaje por negocio (Fase B — prompt IA)

La IA debe adaptarse al tipo de negocio. Ejemplo Joyca Tech: HP, Dell, SSD, RAM, cargadores…

Incluir en contexto Gemini:

```json
{
  "topCategories": [{ "id": "...", "name": "Cargadores" }],
  "topBrands": ["HP", "Dell", "Lenovo"],
  "recentProducts": [{ "name": "...", "catalogItemId": "..." }],
  "commonDimensions": ["Marca", "Capacidad", "Color"]
}
```

**Backend:** query agregada por negocio antes de llamar IA (cache 1h). Menos errores, menos tokens, sugerencias más rápidas.

---

### 23.7 Regla estratégica de velocidad

```text
90% de las entradas de inventario → completar en < 15 segundos desde el escaneo.
```

| Flujo | Cómo cumplir |
|-------|----------------|
| Modo caja existente | Scan → qty → Enter (razón default Compra) |
| Producto nuevo | Solo cuando 404; resumen en 1 pantalla |
| Costo opcional | Colapsado; no bloquear guardar |
| IA | Solo Fase B; nunca en camino crítico de caja |

Toda decisión UX futura se evalúa contra esta regla.

---

### 23.8 Mapa gap vs código actual (jun 2026)

| Recomendación | Estado actual | Fase |
|---------------|---------------|------|
| Descarga APK web | No existe | **A** |
| `GET /api/mobile/app/release` | No existe | **A** |
| Costo en ingreso | Solo adjust stock | **A API** / receive |
| Reason enum | String libre en Notes | **A app** / **M1 API** |
| Resumen confirmación | No en app (web parcial) | **A Flutter** |
| `IsPublic` imágenes | No existe | **M1 migración** |
| Auditoría UserId / stock before | No en movement | **M1 migración** |
| Contexto IA por negocio | No existe | **B** |
| Regla 15 segundos | Norma doc | **A diseño** |

---

## 24. Changelog v2.1 (post-revisión Chat)

| Tema | v2.0 | v2.1 |
|------|------|------|
| Distribución APK | mencionada | **§23.0 diseño completo web + API + CI** |
| Costo en ingreso | — | **§23.1 receive + política costo** |
| Reason enum | — | **§23.2** |
| Resumen confirmación | — | **§23.3 ProductSummaryScreen** |
| IsPublic imágenes | — | **§23.4 migración M1** |
| Auditoría movement | — | **§23.5 M1** |
| IA contexto negocio | genérico | **§23.6 topBrands/categories** |
| Regla 15 s | — | **§23.7 + §0** |

---

## 25. Changelog v2.2 — fotos vs Gemini

| Tema | v2.1 | v2.2 |
|------|------|------|
| Mejora visual fotos | mezclado con IA | **ImageSharp solo** — §16 pipeline explícito |
| Gemini y fotos | vision genérico | **solo lectura → JSON**; no modifica imágenes |
| Recorte fondo IA | Fase C móvil | **Fuera de alcance móvil**; Sprint C ZIP opcional futuro |
| Estilo default | marketplace-white-v1 | **Confirmado suficiente** estilo Amazon |
