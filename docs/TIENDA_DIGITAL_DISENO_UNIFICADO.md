# Tienda digital pública — Diseño unificado (MiNegocioCR)

> **Spec oficial para desarrollo.** Fusiona `TIENDA_DIGITAL_SPEC.md` (Claude) + `MARKETPLACE_LITE_DISENO_v1.md` (Chat).  
> **Inicio implementación (jun 2026):** [MARKETPLACE_INICIO_JUNIO_2026.md](./MARKETPLACE_INICIO_JUNIO_2026.md)  
> Decisiones cerradas en este documento; los otros dos quedan como referencia histórica.

| Decisión | Valor acordado |
|----------|----------------|
| URL pública | `https://www.mi-negociocr.com/tienda/{slug}` |
| Pedidos | `StoreOrder` + `StoreOrderItem` (no `Orders` genérico; no confundir con `RepairOrder`) |
| Ítems del catálogo | `CatalogVariantId` + snapshot nombre/precio/opciones |
| Clientes | Tabla existente `Contacts` (buscar/crear por `BusinessId` + teléfono) |
| Cierre comercial | `Pending` → `Reviewed` → **`Converted`** (`Sale` con `Source = "StoreOrder"`) \| `Cancelled` |
| MVP checkout | **Carrito con varios ítems** (estado en frontend; un solo `POST` al confirmar) |
| Pagos en línea | **No** en MVP |
| Notificaciones MVP | WhatsApp + email (flags del `Business`) + listado de órdenes en panel tenant |
| SEO / redes | Meta dinámicos + campos de tienda ampliados (ver Módulo 1) |
| **Estado código (jun 2026)** | **Sin implementar** — inventario en prod como prerrequisito |

---

## 1. Objetivo

Cada negocio (tenant) comparte un enlace público. El cliente final, **sin login**:

- Navega catálogo (categorías, búsqueda, paginación).
- Ve detalle con galería y variantes.
- Arma un **carrito con varios productos**.
- Envía solicitud con nombre y teléfono.
- El negocio recibe aviso y gestiona la orden hasta convertirla en venta oficial.

---

## 2. Arquitectura

### Stack (sin cambios)

- **API:** .NET 8, Clean Architecture, EF Core, PostgreSQL (Supabase), Railway.
- **Frontend:** Angular 21, Vercel, rutas públicas sin JWT.
- **Multi-tenant:** siempre `BusinessId` en servidor; el slug público resuelve el negocio.

### Capas API

```
/API/Controllers/PublicStoreController.cs   → [AllowAnonymous] /api/public/store/{slug}/...
/API/Controllers/StoreController.cs         → [Authorize] gestión tenant
/Application/UseCases/Store/...
/Domain/Entities/StoreOrder.cs, BusinessStoreSettings.cs
```

### Rutas frontend (públicas)

| Ruta | Pantalla |
|------|----------|
| `/tienda/:slug` | Landing: logo, bienvenida, categorías, grid productos |
| `/tienda/:slug/producto/:itemId` | Detalle, variantes, cantidad, agregar al carrito |
| `/tienda/:slug/carrito` | Carrito multi-ítem |
| `/tienda/:slug/checkout` | Datos del cliente + resumen |
| `/tienda/:slug/confirmacion/:orderId` | Gracias + teléfono del negocio |

Rutas tenant (auth, layout actual):

| Ruta | Pantalla |
|------|----------|
| `/settings/store` o menú **Catálogo digital** | Config tienda + link para compartir |
| `/store/orders` | Lista de pedidos recibidos |
| `/store/orders/:id` | Detalle, cambiar estado, convertir a venta |

---

## 3. Reutilizar del sistema actual

**No crear tablas duplicadas para catálogo ni clientes.**

| Entidad | Uso en tienda |
|---------|----------------|
| `CatalogItem`, `CatalogVariant`, `CatalogImage`, `CatalogCategory` | Catálogo público |
| `Contact` | Cliente del pedido |
| `Sale`, `SaleItem` | Al convertir orden |
| `Business` | Logo, teléfono, descripción, flags WA/email |

**Ocultar en API pública:** `CostPrice`, `ProfitMargin`, tokens, `BusinessId` en JSON.

---

## 4. Modelo de datos nuevo

### `BusinessStoreSettings` (1:1 con `Business`)

```csharp
public class BusinessStoreSettings
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }

    public string Slug { get; set; } = string.Empty;      // único global, a-z0-9-, max 50
    public bool IsStoreEnabled { get; set; } = true;

    public string? WelcomeMessage { get; set; }
    public string? BrandColor { get; set; }               // hex principal
    public string? BrandColorSecondary { get; set; }      // hex secundario (Chat)
    public string? BannerImageUrl { get; set; }           // opcional Supabase

    // Redes (Chat Módulo 8) — opcionales en MVP
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? PublicWhatsApp { get; set; }           // wa.me link; si null usar Business.Phone

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Business Business { get; set; } = null!;
}
```

Índice: `UNIQUE (Slug)`.

Logo y nombre público: preferir `Business.LogoUrl` y `Business.Name`; override de “nombre público” solo si más adelante hace falta.

### `StoreOrder`

```csharp
public enum StoreOrderStatus
{
    Pending = 0,
    Reviewed = 1,
    Converted = 2,
    Cancelled = 3
}

public class StoreOrder
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerNote { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public StoreOrderStatus Status { get; set; } = StoreOrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
    public Business Business { get; set; } = null!;
}
```

Índice: `(BusinessId, Status, CreatedAt DESC)`.

### `StoreOrderItem`

```csharp
public class StoreOrderItem
{
    public Guid Id { get; set; }
    public Guid StoreOrderId { get; set; }
    public Guid CatalogVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string? VariantDescription { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    public StoreOrder StoreOrder { get; set; } = null!;
    public CatalogVariant Variant { get; set; } = null!;
}
```

---

## 5. Carrito multi-ítem (frontend)

### Comportamiento

- Estado del carrito en **sessionStorage** (clave: `store-cart:{slug}`) para no perder al refrescar en la misma sesión.
- Línea de carrito: `variantId`, `catalogItemId`, `productName`, `variantDescription`, `imageUrl`, `unitPrice`, `quantity`, `maxQuantity` (si `TrackStock`).
- Agregar desde detalle: validar variante seleccionada y stock.
- Carrito: editar cantidades, eliminar líneas, ver subtotal/total.
- Checkout: un solo `POST` con todos los ítems (backend ya multi-ítem).

### Límites MVP

- Máximo **20 líneas** distintas en carrito (anti-abuso).
- Cantidad mínima 1 por línea.
- Si `TrackStock`, no permitir cantidad > `StockQuantity` en UI y revalidar en API.

---

## 6. API — endpoints

### 6.1 Tenant (`[Authorize]`)

```
POST   /api/store/settings
GET    /api/store/settings
POST   /api/store/settings/generate-slug

GET    /api/store/orders?status=&dateFrom=&dateTo=&search=&page=
GET    /api/store/orders/{orderId}
PATCH  /api/store/orders/{orderId}/status
POST   /api/store/orders/{orderId}/convert
```

**`StoreSettingsResponse`** incluye `PublicUrl`: `https://www.mi-negociocr.com/tienda/{slug}`.

**Reglas slug:** único, `a-z0-9-`, max 50; slugify desde `Business.Name` + sufijo numérico si colisión.

### 6.2 Público (`[AllowAnonymous]`)

```
GET   /api/public/store/{slug}
GET   /api/public/store/{slug}/categories
GET   /api/public/store/{slug}/catalog?page=1&pageSize=20&categoryId=&search=
GET   /api/public/store/{slug}/catalog/{itemId}
POST  /api/public/store/{slug}/orders
GET   /api/public/store/{slug}/orders/{orderId}    → estado para pantalla confirmación (opcional)
```

Si `IsStoreEnabled = false` → **404** con mensaje claro.

### 6.3 Crear pedido — reglas

1. Resolver negocio por `slug`.
2. Validar cada `VariantId` pertenece al `BusinessId` (anti-IDOR).
3. `Quantity >= 1`; si `TrackStock`, `StockQuantity >= Quantity` (**no descontar** aún).
4. Buscar `Contact` por teléfono; si no existe, crear; si existe, actualizar nombre/email.
5. Crear `StoreOrder` + ítems con **snapshot** de precio y descripción.
6. Notificar tenant (Módulo 7).
7. Respuesta: `OrderId`, `Total`, mensaje de confirmación.

```csharp
public record CreateStoreOrderRequest(
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string? CustomerNote,
    List<StoreOrderItemRequest> Items
);

public record StoreOrderItemRequest(Guid VariantId, int Quantity);
```

### 6.4 Convertir a venta

- Crear `Sale` con `Source = "StoreOrder"`, ítems como `SaleItem`.
- Si `TrackStock` → descontar stock **aquí**, no al crear pedido.
- `StoreOrder.Status = Converted`; `Contact.LastActivityAt = UtcNow`.

---

## 7. Notificaciones (MVP)

Reutilizar infraestructura existente (`INotificationService` o servicio dedicado):

| Canal | Condición |
|-------|-----------|
| WhatsApp | `Business.EnableWhatsappNotifications` + credenciales Meta |
| Email | `Business.EnableEmailNotifications` + Resend |

**Mensaje sugerido:**

```text
🛒 Nueva solicitud de tienda
👤 {CustomerName} · 📱 {CustomerPhone}
📦 {cantidad líneas} producto(s)
💰 Total: ₡{Total:N0}
📝 {CustomerNote o "Sin nota"}
```

Si no hay canal configurado: **no fallar** el POST; la orden queda en el listado tenant.

**Post-MVP:** fila en actividad del dashboard y/o tabla `Notifications` in-app (Chat Módulo 6).

---

## 8. SEO y compartir (Chat Módulo 9)

En rutas públicas Angular (`Title`, `Meta`, Open Graph):

| Página | Title | OG image |
|--------|-------|----------|
| Landing | `{BusinessName} · Tienda` | Logo o banner |
| Producto | `{ProductName} · {BusinessName}` | Imagen primaria del ítem |

`meta description`: descripción del negocio o producto (truncar ~160 chars).

---

## 9. UX / responsive

Breakpoints obligatorios: **320, 375, 390, 414, 768, 1024 px**. Mobile first.

Performance:

- Lazy load imágenes (`loading="lazy"`).
- Catálogo paginado: **20 productos por página** (API + scroll o paginador).
- Cache catálogo público en API: **5 min** en memoria por `slug` (opcional, Módulo 2).

Seguridad:

- Rate limit: **5 POST /orders por IP por hora** (recomendado).
- Captcha: opcional fase 1.5.
- CORS: origen público ya permitido en `Program.cs`.

---

## 10. Orden de implementación

| # | Entregable | Repo |
|---|------------|------|
| 1 | Entidades + migración EF + índices | API |
| 2 | Módulo config tienda (endpoints + pantalla tenant) | API + FE |
| 3 | API catálogo público | API |
| 4 | UI landing + detalle + **servicio carrito** | FE |
| 5 | UI carrito + checkout + confirmación | FE |
| 6 | POST orden + Contact + validaciones | API |
| 7 | Notificaciones WA/email | API |
| 8 | Gestión órdenes + convertir a Sale | API + FE |
| 9 | SEO meta + smoke tests + doc DEPLOY | FE + docs |

---

## 11. Fases posteriores (no MVP)

| Fase | Features (de ambos docs) |
|------|---------------------------|
| **2** | Cupones, promociones, productos destacados, wishlist, búsqueda avanzada |
| **3** | SINPE, tarjeta, tracking cliente, facturación automática |
| **1.5** | Notificaciones in-app en dashboard; captcha; cache CDN imágenes |

---

## 12. Checklist MVP

### API

- [ ] `BusinessStoreSettings`, `StoreOrder`, `StoreOrderItem` + migración
- [ ] Settings CRUD + slug único
- [ ] `PublicStoreController` catálogo (activos, stock, sin costos)
- [ ] `POST orders` multi-ítem + Contact + rate limit
- [ ] Notificación WA/email
- [ ] Listado/detalle/status/convert tenant
- [ ] Tests: slug, IDOR variant, stock, convert Sale

### Frontend público

- [ ] Rutas `/tienda/:slug` sin auth
- [ ] Carrito sessionStorage multi-ítem
- [ ] Checkout + confirmación
- [ ] SEO básico por ruta
- [ ] Errores: tienda off, sin stock, carrito vacío

### Frontend tenant

- [ ] Menú **Catálogo digital** (config + órdenes)
- [ ] Copiar link público
- [ ] Convertir pedido a venta

---

## 13. Referencias

| Archivo | Rol |
|---------|-----|
| `TIENDA_DIGITAL_SPEC.md` | Detalle técnico original (DTOs, wireframes) |
| `MARKETPLACE_LITE_DISENO_v1.md` | Visión producto original |
| `MiNegocioCR.Api/docs/Inventory-API-Handoff.md` | Catálogo existente |
| `DEPLOY.md` | Deploy + CORS + smoke tests |

---

*Tienda digital — diseño unificado · MiNegocioCR · carrito multi-ítem · `/tienda/{slug}`*
