# Inventario MiNegocioCR — Diseño UX aprobado e plan de implementación (v2)

**Estado:** ✅ Sprints 1–4 + post-sprint en **producción** (jun 2026) — ver [CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md](./CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md)  
| **Siguiente módulo:** [MARKETPLACE_INICIO_JUNIO_2026.md](./MARKETPLACE_INICIO_JUNIO_2026.md)  
**Fecha:** Junio 2026  
**Repositorios:** `mi-negociocr-frontend` (`main`) · `MiNegocioCR.Api` (`master`)  
**Referencia análisis inicial:** [INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md](./INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md)  
**API inventario:** [Inventory-API-Handoff.md](./Inventory-API-Handoff.md)

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Principios UX obligatorios](#2-principios-ux-obligatorios)
3. [Decisiones cerradas](#3-decisiones-cerradas)
4. [Wireframes aprobados (texto)](#4-wireframes-aprobados-texto)
5. [Presentaciones multidimensionales](#5-presentaciones-multidimensionales)
6. [Imágenes (restricción backend)](#6-imágenes-restricción-backend)
7. [Productos vs servicios](#7-productos-vs-servicios)
8. [Precio simple vs avanzado](#8-precio-simple-vs-avanzado)
9. [Lista principal y búsqueda](#9-lista-principal-y-búsqueda)
10. [Categorías inline](#10-categorías-inline)
11. [Orquestación frontend y errores](#11-orquestación-frontend-y-errores)
12. [Rendimiento y carga de datos](#12-rendimiento-y-carga-de-datos)
13. [Estados del ítem](#13-estados-del-ítem)
14. [Reglas al editar después de crear](#14-reglas-al-editar-después-de-crear)
15. [Cambio API mínimo (descripción servicio)](#15-cambio-api-mínimo-descripción-servicio)
16. [Arquitectura Angular](#16-arquitectura-angular)
17. [Plan de sprints](#17-plan-de-sprints)
18. [Migración desde la UI actual](#18-migración-desde-la-ui-actual)
19. [Endpoints futuros (no MVP)](#19-endpoints-futuros-no-mvp)
20. [Referencias de código](#20-referencias-de-código)

---

## 1. Resumen ejecutivo

El módulo de inventario actual (`/inventory`, 6 pestañas, ~1.300 líneas en `products.ts`) expone el modelo interno del backend (catálogo → opciones → variantes → movimientos). Los usuarios lo perciben como **engorroso y difícil**.

**Objetivo:** rediseño completo tipo **Shopify / Loyverse / Square POS**:

- Una pantalla principal centrada en **productos y servicios**.
- Jerga oculta: en UI **«Presentación»** (nunca «Variante»).
- Modo simple por defecto; multidimensional con **matriz previsualizable**.
- Backend existente reutilizado al máximo; **única excepción MVP:** campo `Description` en catálogo.

---

## 2. Principios UX obligatorios

| # | Principio |
|---|-----------|
| 1 | Inventario gira alrededor de **productos**, no de variantes técnicas |
| 2 | Sin jerga: no GUID, no Catalog Item, no Option Value; **Presentación** en lugar de Variante |
| 3 | **Modo simple primero** — el sistema crea `CatalogItem` + variante default sin que el usuario lo vea |
| 4 | **Una sola pantalla principal** — eliminar pestañas Resumen, Opciones, Variantes, Movimientos, Ver inventario |
| 5 | **Stock siempre visible** en lista y detalle |
| 6 | Creación en **modal o wizard máx. 2 pasos** |
| 7 | Presentaciones multidimensionales **generadas automáticamente** (producto cartesiano) |
| 8 | IA «agregar producto» — solo diseño futuro, no MVP |

---

## 3. Decisiones cerradas

| Tema | Decisión |
|------|----------|
| Lista | Productos agrupados + servicios en **una sola vista**; filtros Todos / Productos / Servicios |
| Categoría productos | **Obligatoria** en UX (inline «Nueva categoría») |
| Categoría servicios | **Opcional** |
| Foto servicios MVP | **No** |
| Descripción servicios | **Sí** — requiere DTO API (§15) |
| Precio MVP | **Modo simple default** (`SetPriceManually` + precio venta; costo opcional) |
| Imágenes productos | **Opción C:** foto principal + override opcional por presentación |
| SKU | **AUTO sugerido** editable (`FUNDA-NEG-S`); API permite vacío |
| Matriz defaults | Mismo precio, costo y stock para todas las filas; **tabla editable** fila por fila |
| Stock bajo | Umbral **configurable después**; MVP puede usar 2 fijo en cliente |
| Servicios POS / Créditos | **Fase 2** — MVP solo CRUD en inventario; POS mantiene texto libre + catálogo después |
| Marketplace / tienda | Servicios **ignorados**; catálogo por variantes — [MARKETPLACE_INICIO_JUNIO_2026.md](./MARKETPLACE_INICIO_JUNIO_2026.md) |
| Combinaciones > 50 | **Advertencia** antes de guardar |
| Límite combinaciones | Guardrail ~50 con confirmación (ajustable) |

---

## 4. Wireframes aprobados (texto)

### 4.1 Lista principal

```
🔍 Buscar

[Todos]  [Productos]  [Servicios]

[+ Agregar]

────────────────────────────────

📦 Funda iPhone
6 presentaciones · Stock total: 43 · Precio desde ₡3.500
[Editar]  [+ Stock]

────────────────────────────────

🔧 Mantenimiento PC
🏷 Servicio · Precio ₡20.000
[Editar]

────────────────────────────────

📦 Mouse Logitech
Stock: 12 · Precio ₡8.500
[Editar]  [+ Stock]
```

### 4.2 Agregar producto (simple)

```
Agregar producto

Nombre          [____________]
Categoría       [____________ ▼]  → Nueva categoría
Precio venta    [____________]
Costo           [ Opcional ]
Stock inicial   [____________]

¿Tiene presentaciones?
( ) No    ( ) Sí

[ Guardar ]
```

Si **Sí** → continuar a flujo multidimensional (§5).

### 4.3 Agregar servicio

```
Agregar servicio

Nombre          [____________]
Descripción     [____________]
Precio          [____________]
Categoría       [ Opcional ▼ ]

[ Guardar ]
```

### 4.4 Detalle producto

```
Funda iPhone
Stock total: 43

Presentaciones

Negro - S    Stock: 5     ₡3.500
Negro - M    Stock: 10    ₡3.500
Negro - L    Stock: 8     ₡3.500
...

[ Ajustar stock ]  [ Editar producto ]
```

### 4.5 Progreso de creación (orquestación)

```
Guardando producto...

✓ Producto creado

Creando presentación 1 de 9...
Creando presentación 2 de 9...
...
Subiendo imágenes...

✓ Producto creado correctamente
```

---

## 5. Presentaciones multidimensionales

### Flujo

**Paso 1 — Dimensiones**

Usuario define dimensiones y valores, ej.:

- Color: Negro, Azul, Rojo
- Talla: S, M, L

**Paso 2 — Vista previa**

Sistema muestra: «Se crearán **9** presentaciones» y lista combinaciones.

**Paso 3 — Matriz editable (tipo Shopify)**

| Presentación | Precio | Costo | Stock | SKU |
|--------------|--------|-------|-------|-----|
| Negro - S | 3500 | 2000 | 10 | AUTO |
| Negro - M | 3500 | 2000 | 10 | AUTO |
| … | … | … | … | … |

- **Defaults:** mismo precio, costo y stock en todas las filas (90% de casos).
- **Override:** cualquier fila editable antes de guardar.
- **SKU AUTO:** prefijo del nombre + abreviaturas (`FUNDA-NEG-S`); editable.

### Orquestación API (sin cambio backend)

Por dimensión: `POST /options` + N × `POST /option-values`  
Por fila de matriz: `POST /variants` con `optionValueIds` de esa combinación.

Casos soportados: solo color · solo talla · color+talla · color+talla+capacidad.

### Guardrail

Si combinaciones > **50** → modal: «Se crearán N presentaciones. ¿Continuar?»

---

## 6. Imágenes (restricción backend)

Las imágenes viven en **`CatalogVariant`**, no en el producto padre.

| API | Límite |
|-----|--------|
| `POST /variants/{variantId}/images` | Máx. 3, 5 MB, png/jpeg |
| `GET /variants/{variantId}/images` | — |

### Opción C (aprobada)

| Caso | UX usuario | Implementación |
|------|------------|----------------|
| Producto simple | «Fotos del producto» | Subir a variante default |
| Varias presentaciones | Foto principal del producto | Frontend **repite** upload en cada presentación al crear |
| Detalle | Cambiar foto de una presentación | Upload solo a esa `variantId` |

**Servicios MVP:** sin foto.

---

## 7. Productos vs servicios

### Backend

| Tipo | `CatalogItemType` | Stock | Presentaciones |
|------|-------------------|-------|----------------|
| Producto | `1` | Sí | Opcional (variantes) |
| Servicio | `2` | No | No |

### MVP inventario

| Acción | Producto | Servicio |
|--------|----------|----------|
| Crear | ✅ | ✅ |
| Editar | ✅ | ✅ |
| Listar | ✅ | ✅ |
| Activar / Desactivar | ✅ | ✅ |
| Foto | ✅ (variante) | ❌ |
| + Stock / ajustes | ✅ | ❌ |

### Fase 2 (no mezclar con refactor inventario)

- POS: buscar servicio en catálogo **y** mantener texto libre.
- Créditos: agregar servicio desde catálogo (hoy: `FreeConcept` con texto).
- Marketplace: ignorar servicios.

### Creación servicio

Solo `POST /api/catalog` con `type: 2`, `basePrice`, `trackStock: false`, sin variante.

---

## 8. Precio simple vs avanzado

### Modo simple (DEFAULT)

```
Precio de venta  [ ₡________ ]
Costo            [ Opcional  ]
```

- `SetPriceManually: true`, `CostPrice: 0` permitido.
- No obligar margen ni costo.

### Modo avanzado (colapsado)

```
[ Mostrar cálculo avanzado ]

Costo → Margen % → IVA (negocio) → Precio final (₡5 ceil)
```

Reutilizar `variant-sale-price.ts` y `CatalogVariantPriceResolver` en backend.

---

## 9. Lista principal y búsqueda

### Fuentes de datos

| Tipo | Endpoint | Agrupación |
|------|----------|------------|
| Productos | `GET /variants/business/{businessId}?search=` | Agrupar por `catalogItemId` en frontend |
| Servicios | `GET /catalog/mine` filtro `type=2` | Una fila por servicio |

**Evitar N+1:** no iterar `GET /variants/{catalogItemId}` por producto.

### Campos en lista (producto con presentaciones)

- Cantidad de presentaciones
- Stock total (suma)
- Precio: «Desde ₡X» o «₡X - ₡Y»

### Búsqueda ultra rápida

`search` en variantes ya cubre: nombre producto, SKU, valores de presentación (ej. «negro»).

Categoría: filtro cliente con datos de catálogo (el `search` del API no incluye nombre de categoría hoy).

Ejemplo: buscar `negro` → filas agrupadas bajo «Funda iPhone» con presentaciones Negro-M, Negro-L.

---

## 10. Categorías inline

- Selector con opción **«Nueva categoría»** → modal → `POST /categories` → auto-seleccionar.
- Si no hay categorías: «Aún no tienes categorías» + [Crear primera categoría] **dentro del formulario**.
- Administración opcional: Inventario → Categorías (pantalla secundaria).
- **Productos:** categoría obligatoria en UX.
- **Servicios:** categoría opcional.

---

## 11. Orquestación frontend y errores

### Secuencia producto simple

1. `POST /catalog`
2. `POST /variants` (presentación default)
3. (opcional) `POST .../images`

### Escenarios

| Escenario | Resultado UX |
|-----------|--------------|
| A — todo OK | Éxito |
| B — ítem OK, presentación falló | **Incompleto** + [Completar] + [Reintentar] |
| C — ítem + presentación OK, imágenes fallaron | Éxito parcial + [Reintentar imágenes] |

### Reglas UX

- Un solo estado: «Guardando producto…» (no mostrar cada HTTP).
- No perder datos del formulario en error.
- Reintentos seguros en presentaciones fallidas (no recrear ítem).
- Diseño preparado para futuro `POST /catalog/quick-product` (transacción única).

---

## 12. Rendimiento y carga de datos

- Pantalla principal: **1 request** variantes (+ 1 catálogo para servicios, en paralelo).
- Búsqueda con debounce (~300 ms).
- Lista virtual scroll si > 100 filas agrupadas.
- Paginación API: **futuro** si miles de presentaciones.

---

## 13. Estados del ítem

| Estado | Detección (frontend) | UI |
|--------|----------------------|-----|
| **Activo** | `isActive` | Normal |
| **Inactivo** | `!isActive` | Atenuado / badge |
| **Incompleto** | Producto sin presentaciones o creación parcial interrumpida | ⚠ + [Completar] |
| **Stock bajo** | Alguna presentación `currentStock <= umbral` | ⚠ Stock bajo |

Umbral: configurable después; MVP default **2**.

---

## 14. Reglas al editar después de crear

1. **No eliminar** presentaciones con ventas/compras/movimientos (API lo bloquea) — mensaje claro.
2. **Stock:** preferir [+ Stock] / ajuste con motivo, no edición directa del número histórico.
3. **Nuevas presentaciones:** solo combinaciones que **no existan**; recalcular matriz sin duplicar.
4. **Precio/costo:** `PUT /variants/{id}`; no cambia stock.
5. **Desactivar** producto no elimina presentaciones.

---

## 15. Cambio API mínimo (descripción servicio)

**Única excepción** al «sin cambios backend» en MVP inventario.

| Archivo / DTO | Cambio |
|---------------|--------|
| `CreateCatalogItemRequestDto` | `string? Description` |
| `UpdateCatalogItemRequestDto` | `string? Description` |
| `CatalogItemDto` | `string? Description` en GET |
| `CreateCatalogItemUseCase` | Persistir `Description` en entidad (ya existe columna) |

Sin migración EF si la columna `Description` ya está en `CatalogItems`.

---

## 16. Arquitectura Angular

### Eliminar / retirar

| Componente / archivo | Motivo |
|----------------------|--------|
| `pages/products/products.ts` monolito + 6 pestañas | Reemplazo total |
| `pages/create-product/`, `pages/edit-product/` | Mock muerto |
| `services/product.service.ts` | Mock |
| UI separada `OptionsManagement` + `OptionValuesManagement` como tabs | Absorbido en wizard |

### Crear

| Componente / servicio | Rol |
|-----------------------|-----|
| `inventory-product-list` | Pantalla principal `/inventory` |
| `product-quick-add-dialog` | Alta producto simple + bifurcación presentaciones |
| `service-quick-add-dialog` | Alta servicio |
| `presentation-dimensions-step` | Definir dimensiones |
| `presentation-matrix-preview` | Matriz editable pre-guardado |
| `product-detail` | Detalle + lista presentaciones + stock |
| `stock-quick-adjust-dialog` | +/- stock con motivo |
| `category-inline-create-dialog` | Nueva categoría sin salir del flujo |
| `inventory-orchestrator.service` | Encadena catalog → options → variants → images |
| `inventory-product-aggregator.service` | Fusiona variantes + servicios para lista |

### Reutilizar

- `CatalogService`, `VariantsService`, `CategoriesService`, `OptionsService`, `OptionValuesService`, `InventoryMovementsService`
- `variant-sale-price.ts`, `BusinessConfigService`, `InventoryConfigService`
- `VariantItemImagesComponent` (adaptado)
- `VariantSummary` — **no romper** POS, créditos, reparaciones

### Rutas propuestas

| Ruta | Componente |
|------|------------|
| `/inventory` | `inventory-product-list` |
| `/inventory/product/:id` | `product-detail` (opcional; puede ser panel/modal) |
| `/inventory/categories` | Admin categorías (opcional P2) |

---

## 17. Plan de sprints

### Sprint 1 — Fundación (MVP visible)

| # | Entregable | Estado |
|---|------------|--------|
| 1.1 | API: `Description` en DTOs catalog | ✅ |
| 1.2 | `inventory-product-aggregator.service` + carga `variants/business` + `catalog` | ✅ |
| 1.3 | Pantalla lista: buscador, filtros Todos/Productos/Servicios, estados básicos | ✅ |
| 1.4 | Modal **Agregar producto** simple (orquestador: catalog + variante default) | ✅ |
| 1.5 | Modal **Agregar servicio** | ✅ |
| 1.6 | Categoría inline + primera categoría | ✅ |
| 1.7 | Precio modo simple (`SetPriceManually`) | ✅ |
| 1.8 | Ruta `/inventory` → `inventory-list` (legacy `products` sin usar) | ✅ |
| 1.9 | [+ Stock] rápido + estado Incompleto / Completar | ✅ |

**Criterio de éxito:** usuario crea mouse o servicio «Mantenimiento PC» en < 1 min sin ver pestañas técnicas.

### Sprint 2 — Presentaciones y robustez ✅ (Jun 2026)

| # | Entregable | Estado |
|---|------------|--------|
| 2.1 | Wizard paso 2: dimensiones + generar combinaciones | ✅ |
| 2.2 | Matriz preview editable + SKU AUTO | ✅ |
| 2.3 | Progreso «Creando presentación X de N» | ✅ |
| 2.4 | Estado **Incompleto** + [Completar] + reintentos | ✅ (Sprint 1) |
| 2.5 | Guardrail > 50 combinaciones | ✅ |
| 2.6 | Detalle producto con lista de presentaciones | ✅ |

**Criterio de éxito:** Funda iPhone 3×3 colores/tallas en un flujo con preview antes de guardar.

**Changelog FE:** `mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT2_JUNIO_2026.md`

### Sprint 3 — Pulido operativo ✅ (Jun 2026)

| # | Entregable | Estado |
|---|------------|--------|
| 3.1 | [+ Stock] y diálogo ajuste desde lista y detalle | ✅ |
| 3.2 | Editar presentación (precio, costo, SKU) | ✅ |
| 3.3 | Imágenes opción C (principal + override) | ✅ |
| 3.4 | Modo precio avanzado colapsado | ✅ |
| 3.5 | Badge stock bajo (umbral 2; hook para configurable) | ✅ |
| 3.6 | Activar / desactivar producto y servicio | ✅ |
| 3.7 | Eliminar código muerto (`create-product`, mock) | ✅ |
| 3.8 | Doc + pruebas manuales checklist | ✅ |

**Criterio de éxito:** operación diaria completa sin volver a UI legacy.

**Changelog FE:** `mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT3_JUNIO_2026.md`

### Sprint 4 — Grid, filtros y pulido ✅ (Jun 2026)

| # | Entregable | Estado |
|---|------------|--------|
| 4.1 | Lista en **grid responsive** (1–4 columnas) | ✅ |
| 4.2 | Toolbar compacta + fondo gris / tarjetas blancas | ✅ |
| 4.3 | Filtro **Inactivos** (productos + servicios) | ✅ |
| 4.4 | Fix clasificación **producto vs servicio** (enum string API) | ✅ |
| 4.5 | Estados stock: disponible / bajo / sin stock (promedio multi-presentación) | ✅ |
| 4.6 | Acciones: Ver, Editar (1 pres.), Completar, Activar | ✅ |
| 4.7 | Fix guardar producto simple (form dimensiones ocultas) | ✅ |
| 4.8 | Detalle: hero foto `contain`, margen guardado al editar | ✅ |
| 4.9 | API: toggle presentación, márgenes en listado, fixes repositorio | ✅ |

**Criterio de éxito:** operación diaria en grid denso; servicios separados de productos; inactivos visibles en un solo filtro.

**Changelog:** `mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md` · API: `MiNegocioCR.Api/docs/CAMBIOS_INVENTARIO_API_JUNIO_2026.md`

### Fase 2 — Integración POS / Créditos (release aparte)

| # | Entregable |
|---|------------|
| F2.1 | POS: buscar servicios del catálogo además de texto libre |
| F2.2 | Créditos: línea desde catálogo de servicios |
| F2.3 | Umbral stock bajo configurable en settings negocio |
| F2.4 | (Opcional) `POST /catalog/quick-product` transaccional |

---

## 18. Migración desde la UI actual

| Paso | Acción |
|------|--------|
| 1 | Implementar lista nueva en paralelo tras feature flag o reemplazo directo en `/inventory` |
| 2 | Mantener servicios HTTP sin breaking changes |
| 3 | No migrar datos — mismo backend |
| 4 | Usuarios con productos existentes: lista nueva los muestra vía `variants/business` |
| 5 | Eliminar `products.ts` legacy al cerrar Sprint 3 |
| 6 | Actualizar `CAMBIOS_*` y este doc al deploy |

---

## 19. Endpoints futuros (no MVP)

| Endpoint | Beneficio |
|----------|-----------|
| `POST /api/catalog/quick-product` | Creación atómica ítem + presentación + stock |
| `GET /api/inventory/movements?variantId=` | Historial |
| `GET /api/variants/business/{id}?lowStockOnly=true` | Filtro alertas server-side |
| `search` incluyendo nombre categoría | Búsqueda unificada |

---

## 20. Referencias de código

### Frontend actual (a reemplazar)

```
mi-negociocr-frontend/src/app/features/inventory/
├── pages/products/products.ts      ← monolito legacy
├── pages/create-product/           ← eliminar
├── pages/edit-product/             ← eliminar
└── services/product.service.ts     ← eliminar
```

### Backend

```
MiNegocioCR.Api/
├── Application/UseCases/Repository/CreateVariantUseCase.cs
├── Application/UseCases/Repository/CreateCatalogItemUseCase.cs
├── Application/Common/CatalogVariantPriceResolver.cs
└── API/Controllers/VariantController.cs  → GET variants/business
```

### Documentación relacionada

| Doc | Uso |
|-----|-----|
| [INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md](./INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md) | Análisis estado actual |
| [Inventory-API-Handoff.md](./Inventory-API-Handoff.md) | Contrato REST |
| [crc-sale-price-rounding.md](./crc-sale-price-rounding.md) | Precios ₡5 |

---

*Documento v2 — Junio 2026 · Diseño aprobado · Listo para implementación*
