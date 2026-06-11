# Marketplace / Tienda digital — Inicio de implementación (Junio 2026)

> **Empezar aquí.** Spec funcional y técnica completa: **[TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md)**  
> Visión producto original: [MARKETPLACE_LITE_DISENO_v1.md](./MARKETPLACE_LITE_DISENO_v1.md) · Detalle DTOs: [TIENDA_DIGITAL_SPEC.md](./TIENDA_DIGITAL_SPEC.md)

**Fecha:** 11 junio 2026  
**Estado:** Inventario y módulos core en **producción** — **marketplace sin código aún** (backlog → activo)  
**Repos:** API `master` · Frontend `main`

---

## 1. Qué vamos a construir

Tienda pública por negocio, **sin login del cliente final**:

| URL | Pantalla |
|-----|----------|
| `/tienda/{slug}` | Landing: logo, categorías, grid de productos |
| `/tienda/{slug}/producto/{itemId}` | Detalle, variantes, agregar al carrito |
| `/tienda/{slug}/carrito` | Carrito multi-ítem |
| `/tienda/{slug}/checkout` | Nombre, teléfono, notas |
| `/tienda/{slug}/confirmacion/{orderId}` | Gracias |

Panel tenant (auth):

| Ruta propuesta | Pantalla |
|----------------|----------|
| `/settings/store` o menú **Catálogo digital** | Slug, colores, activar tienda, copiar link |
| `/store/orders` | Pedidos recibidos |
| `/store/orders/:id` | Detalle, estados, **convertir a venta** |

**MVP:** carrito multi-ítem, un `POST` al confirmar, **sin pago en línea**.  
**Entidades nuevas:** `BusinessStoreSettings`, `StoreOrder`, `StoreOrderItem` (no reutilizar `RepairOrder` ni `InternetOrder`).

---

## 2. Prerrequisitos cerrados (no bloquean)

| Módulo | Estado | Relevancia para tienda |
|--------|--------|------------------------|
| Inventario UX (sprints 1–4 + post-sprint) | ✅ Producción | Catálogo, variantes, fotos, precios, stock |
| Ventas / POS | ✅ Producción | Conversión `StoreOrder` → `Sale` |
| Contactos CRM | ✅ Producción | Cliente por teléfono en checkout |
| WhatsApp / Email | ✅ Producción | Notificar nueva orden |
| Créditos, Reparaciones, Pedidos Internet | ✅ Producción | Sin conflicto; catálogo compartido solo productos |

### Reglas de catálogo para la tienda pública

- Mostrar solo **`CatalogItem` activos** tipo **Producto** (`CatalogItemType.Product`).
- **Servicios:** no aparecen en tienda ([INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md) §3).
- Vender por **`CatalogVariant`**: precio de venta, stock, imagen primaria.
- **No exponer** en API pública: `CostPrice`, `ProfitMargin`, `BusinessId`, tokens.

---

## 3. Lo que NO existe todavía (grep jun 2026)

- Entidades `BusinessStoreSettings`, `StoreOrder`, `StoreOrderItem`
- `PublicStoreController` / `StoreController`
- Rutas Angular `/tienda/*`
- Menú tenant **Catálogo digital**
- Servicio carrito (`sessionStorage`: `store-cart:{slug}`)

---

## 4. Orden de implementación recomendado

Seguir §10 de [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md):

| Fase | Entregable | Repo | Depende de |
|------|------------|------|------------|
| **M1** | Entidades + migración EF + índices (`Slug` único) | API | — |
| **M2** | Config tienda tenant (`GET/POST /api/store/settings`, generar slug) | API + FE | M1 |
| **M3** | Catálogo público (`GET /api/public/store/{slug}/...`) | API | M1, inventario existente |
| **M4** | UI landing + detalle + servicio carrito | FE | M3 |
| **M5** | Carrito + checkout + confirmación | FE | M4 |
| **M6** | `POST` orden multi-ítem + Contact + validación stock | API | M1, M3 |
| **M7** | Notificaciones WA / email | API | M6 |
| **M8** | Gestión órdenes + convertir a `Sale` | API + FE | M6, ventas |
| **M9** | SEO meta + smoke tests + DEPLOY | FE + docs | M4–M8 |

### Primera tarea concreta (M1)

1. Crear entidades en `Domain/Entities/` según spec unificada §4.
2. Configurar EF en `Infrastructure/Persistence/`.
3. Migración + script idempotente en `Scripts/apply-store-migration.sql`.
4. Tests: slug único, FK `BusinessId`, cascade ítems.

---

## 5. Estructura de código planificada

### API

```
Domain/Entities/
  BusinessStoreSettings.cs
  StoreOrder.cs
  StoreOrderItem.cs
Application/UseCases/Store/
  CreateStoreSettingsUseCase.cs
  GetPublicStoreCatalogUseCase.cs
  CreateStoreOrderUseCase.cs
  ConvertStoreOrderToSaleUseCase.cs
  ...
API/Controllers/
  PublicStoreController.cs      [AllowAnonymous]
  StoreController.cs              [Authorize]
```

### Frontend

```
src/app/features/store/           # panel tenant (órdenes, config)
src/app/features/public-store/    # rutas sin layout auth
  pages/store-landing/
  pages/store-product/
  pages/store-cart/
  pages/store-checkout/
  pages/store-confirmation/
  services/store-cart.service.ts
```

Registrar rutas en `app.routes.ts` **fuera** del guard de auth para `/tienda/:slug/**`.

---

## 6. Commits de referencia (pre-marketplace)

| Repo | Commit | Notas |
|------|--------|-------|
| Frontend | `3b1f700` | Dimensiones en agregar presentación, quick-add margen por fila, errores ES |
| API | `f517a01` | Errores ES al borrar dimensión/valor en uso |

Changelog inventario reciente: [CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md](./CAMBIOS_INVENTARIO_JUNIO_2026_DIMENSIONES_Y_PRECIO.md)

---

## 7. Checklist antes de merge MVP tienda

### API

- [ ] Migración aplicada en prod (`Scripts/apply-store-migration.sql`)
- [ ] Slug único global; tienda desactivada → 404
- [ ] IDOR: variantes solo del `BusinessId` del slug
- [ ] Stock validado en POST; **no descontar** hasta convertir
- [ ] Rate limit POST órdenes (~5/IP/hora)
- [ ] Convert → `Sale` con `Source = "StoreOrder"`

### Frontend público

- [ ] Mobile first (320–1024 px)
- [ ] Carrito persiste en sesión
- [ ] SEO básico (title, OG)

### Frontend tenant

- [ ] Copiar link `https://www.mi-negociocr.com/tienda/{slug}`
- [ ] Listado y detalle de órdenes

### Smoke post-deploy

- [ ] Tienda pública carga productos con foto y precio
- [ ] Checkout crea orden; aparece en panel tenant
- [ ] Convertir orden genera venta y baja stock

---

## 8. Documentación relacionada

| Documento | Uso |
|-----------|-----|
| [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md) | Spec oficial — leer completo antes de codificar |
| [Inventory-API-Handoff.md](./MiNegocioCR.Api/docs/Inventory-API-Handoff.md) | Catálogo/variantes existentes |
| [DEPLOY.md](./DEPLOY.md) | Railway + Vercel + CORS |
| [WORKSPACE_INDEX.md](./MiNegocioCR.Api/docs/WORKSPACE_INDEX.md) | Índice general del monorepo |

---

*Marketplace / tienda digital — kickoff · MiNegocioCR · Junio 2026*
