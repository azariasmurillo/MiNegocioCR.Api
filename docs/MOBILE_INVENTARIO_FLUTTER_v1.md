# App móvil Flutter — Inventario rápido (diseño v1)

> **Superseded:** usar [MOBILE_INVENTARIO_FLUTTER_v2.md](./MOBILE_INVENTARIO_FLUTTER_v2.md) (v1 + mejoras estratégicas).

> **Spec para desarrollo** de la app Android que complementa MiNegocioCR web.> Reutiliza el modelo de inventario existente (`CatalogItem` → `CatalogVariant` → imágenes / stock).  
> Complementa [VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md](./VARIANT_IMAGE_IMPORT_MARKETPLACE_v1.md) y [INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md).

| Decisión | Valor |
|----------|--------|
| Plataforma inicial | **Android** (Flutter) |
| Distribución | Descarga APK desde panel MiNegocioCR |
| Auth | Mismo JWT que la web (`POST /api/auth/login`) |
| Match barcode | **SKU único por negocio** (barcode = SKU por convención) |
| Fotos nuevas | Máx. **3** por variante, enhancer marketplace |
| IA clasificación | **Fase B** (Gemini Flash vision) — no MVP |
| Recorte fondo IA | **Fase B/C** — Sprint C API (`useBackgroundRemoval`) |

**Estado código (jun 2026):** diseño cerrado; **`GET /api/variants/by-sku/{sku}`** implementado (Sprint M0).

---

## 1. Visión

App en el celular del comerciante para:

1. **Escanear código de barras** → saber si el SKU ya existe en su negocio.
2. Si **existe**: agregar stock y/o actualizar fotos (mejoradas tipo marketplace).
3. Si **no existe**: capturar hasta 3 fotos, completar datos (costo, precio/margen, stock) y crear producto en el catálogo.
4. **(Fase B)** La IA sugiere nombre, categoría y si el artículo encaja en un producto/presentación existente; el usuario **siempre confirma** antes de guardar.

---

## 2. Qué reutilizamos del backend actual

| Capa | Endpoint / componente | Uso móvil |
|------|----------------------|-----------|
| Login | `POST /api/auth/login` | Sesión JWT |
| Sesión | `GET /api/auth/me` | Refrescar usuario |
| Config negocio | `GET /api/businesses/{id}/config` | Margen default, IVA |
| Lookup SKU | `GET /api/variants/by-sku/{sku}` | **Post escaneo barcode** |
| Listado | `GET /api/variants/business/{id}?search=` | Búsqueda manual |
| Crear producto | `POST /api/catalog` + `POST /api/variants` | Producto nuevo |
| Categorías | `GET /api/catalog-categories/...` | Dropdown |
| Stock | `POST /api/inventory/adjust` | Sumar unidades |
| Fotos | `POST /api/variants/{id}/images` | Upload PNG/JPEG |
| Enhancer | Pipeline ImageSharp + WebP 3 tamaños | Vía endpoints móvil (pendiente M1) |
| SKU único | `FindByBusinessAndSkuAsync` | Validación tenant |

---

## 3. Fases de entrega

### Fase M0 — Fundación API ✅ (este sprint)
- [x] `GET /api/variants/by-sku/{sku}` — lookup exacto por tenant (JWT `businessId`)
- [x] Este documento de diseño

### Fase A — MVP Flutter (sin IA)
- [ ] App: login, escáner, flujos existe / no existe
- [ ] `POST /api/mobile/inventory/quick-create` (opcional: orquestar catalog+variant+fotos)
- [ ] `POST /api/mobile/inventory/{variantId}/photos` con enhancer marketplace
- [ ] Página web: enlace descarga APK

### Fase B — IA asistida (Gemini)
- [ ] `POST /api/mobile/inventory/ai-suggest` — fotos → JSON sugerencia (sin persistir)
- [ ] Pantalla revisión humana obligatoria
- [ ] Límites de uso por negocio / plan

### Fase C — Catálogo inteligente
- [ ] IA propone producto nuevo vs nueva presentación
- [ ] Recorte de fondo real (Sprint C API)
- [ ] Campo `barcode` separado de SKU interno (si el negocio lo pide)

---

## 4. Flujos UX

### 4.1 Login
```
Email + contraseña → POST /api/auth/login → guardar token (flutter_secure_storage)
Todas las llamadas: Authorization: Bearer {token}
```

### 4.2 Escaneo — SKU encontrado
```
Barcode → GET /api/variants/by-sku/{sku}
  → 200: pantalla variante (foto, nombre, stock, precio)
       [Agregar stock]  → POST /api/inventory/adjust
       [Actualizar fotos] → POST /api/variants/{id}/images (máx 3)
  → 404: flujo producto nuevo
```

### 4.3 Escaneo — SKU no encontrado
```
Mensaje: «Este producto no está en tu inventario»
  → Hasta 3 fotos (cámara)
  → Formulario: nombre, categoría, SKU (= barcode), costo
  → Precio: fijo O calculado (margen % default del negocio)
  → Stock inicial
  → POST catalog + POST variants + upload imágenes
```

### 4.4 Precio
| Modo | API |
|------|-----|
| Fijo | `setPriceManually: true` |
| Calculado | costo + `profitMargin` + IVA negocio (`CatalogVariantPriceResolver`) |

Default margen: `GET /api/businesses/{id}/config` → `defaultProfitMargin`.

---

## 5. API — `GET /api/variants/by-sku/{sku}` (implementado)

**Auth:** Bearer JWT. `businessId` se toma del token (no confiar en query del cliente).

**Parámetros:**
- `sku` (ruta): código escaneado; comparación case-insensitive con `SkuNormalized`.

**200 OK** — `VariantBySkuLookupDto`:
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

**404** — No hay variante con ese SKU en el negocio.

**400** — SKU vacío o inválido.

---

## 6. API propuesta — Fase A/B (pendiente)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/mobile/inventory/scan` | Wrapper: `{ sku }` → `{ found, variant?, actions[] }` |
| `POST` | `/api/mobile/inventory/quick-create` | Producto simple + variante + fotos (transacción) |
| `POST` | `/api/mobile/inventory/{variantId}/photos` | 1–3 fotos + enhancer `marketplace-white-v1` |
| `POST` | `/api/mobile/inventory/ai-suggest` | **Fase B** — multimodal Gemini, solo sugerencia |

### Ejemplo `ai-suggest` (Fase B)
```json
{
  "confidence": 0.82,
  "suggestedProductName": "Adaptador USB-C 65W",
  "suggestedCategoryId": "uuid-or-null",
  "matchExisting": {
    "catalogItemId": "uuid-or-null",
    "catalogItemName": "Cargadores USB-C",
    "reason": "Misma familia de producto"
  },
  "suggestedPresentation": { "dimensionName": "Marca", "valueText": "Anker" },
  "warnings": []
}
```
**Regla:** la IA nunca persiste; solo `quick-create` tras confirmación del usuario.

---

## 7. Arquitectura Flutter (recomendada)

| Paquete | Uso |
|---------|-----|
| `dio` + interceptor | HTTP + JWT |
| `flutter_secure_storage` | Token |
| `mobile_scanner` | Barcode |
| `image_picker` | Cámara / galería |
| `riverpod` | Estado |

### Pantallas (checklist)
1. LoginScreen  
2. HomeScreen  
3. BarcodeScannerScreen  
4. VariantFoundScreen  
5. ProductNotFoundScreen  
6. PhotoCaptureScreen  
7. ProductFormScreen  
8. AiSuggestionReviewScreen (Fase B)  
9. PhotoPreviewEnhancedScreen  
10. SuccessScreen  

---

## 8. IA con Gemini (Fase B)

| Tarea | Modelo sugerido |
|-------|-----------------|
| Clasificar producto + categoría | Gemini 2.0 Flash (vision) |
| ¿Encaja en producto existente? | Mismo + JSON catálogo del negocio en prompt |

**Contexto en prompt:** categorías del negocio, productos recientes, reglas («preferí nueva presentación vs producto duplicado»).

**Costos:** rate limit por negocio (ej. 50/día plan básico).

---

## 9. Imágenes marketplace

Reutilizar pipeline actual:
- Estilos: `marketplace-white-v1` (default), `marketplace-soft-v1`
- Salida: WebP 1200 / 600 / 300 px
- Storage: Supabase `business-assets`

Fase C: `useBackgroundRemoval` cuando Sprint C API esté listo.

---

## 10. Seguridad

- Solo usuarios activos con `businessId` en JWT.
- SKU lookup acotado al tenant del token.
- Fotos: máx 5 MB, png/jpeg (igual que upload manual).
- MVP online-only; cola offline = fase futura.

---

## 11. Preguntas abiertas (producto)

1. ¿Barcode siempre = SKU o habrá SKU interno distinto?
2. ¿Multidimensional desde el celular o solo producto simple en MVP?
3. ¿IA incluida en plan o con límite/costo extra?
4. ¿Solo entrada de inventario o también consulta precio en piso de venta?
5. ¿iOS después de Android?

---

## 12. Prompt para Gemini (Fase A Flutter)

```
Implementá la Fase A de la app Flutter «MiNegocioCR Inventario Rápido» para Android.

- Login: POST /api/auth/login, JWT en flutter_secure_storage.
- Escaneo: mobile_scanner → GET /api/variants/by-sku/{sku} (businessId del token).
- Si 200: pantalla con stock; POST /api/inventory/adjust; upload fotos.
- Si 404: wizard con POST /api/catalog + POST /api/variants + fotos.
- Precio fijo o calculado; margen default desde GET /api/businesses/{id}/config.
- Máx 3 fotos; SKU único por negocio.
- Sin IA en Fase A.
- Stack: Flutter 3, Riverpod, dio, mobile_scanner, flutter_secure_storage.
```

---

## 13. Referencias de código

| Tema | Archivo |
|------|---------|
| Lookup SKU repo | `VariantRepository.FindByBusinessAndSkuAsync` |
| Use case by-sku | `GetVariantBySkuUseCase` |
| Controller | `VariantController.GetBySku` |
| Import / enhancer | `ImageImportBatchProcessor`, `LocalImageSharpProductImageEnhancerService` |
| Orquestación FE web | `inventory-orchestrator.service.ts`, `product-quick-add-dialog` |
