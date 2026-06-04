# 🛒 Tienda Digital Pública — MiNegocioCR
> Documento de especificación para desarrollo con Cursor AI  
> Proyecto: `MiNegocioCR.Api` (.NET 8 / Clean Architecture / PostgreSQL / Supabase)  
> Sitio actual: [www.mi-negociocr.com](https://www.mi-negociocr.com)

> **Spec oficial unificada:** [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md) — URL `/tienda/{slug}`, carrito multi-ítem, settings ampliados. Este archivo sigue siendo la referencia técnica detallada (DTOs, wireframes, checklist).

---

## 📋 Resumen ejecutivo

Se necesita un **área pública de tienda digital** (tipo marketplace simplificado) accesible mediante una URL única por Tenant/Business. Los clientes finales pueden navegar el catálogo, ver detalles y fotos de productos, y hacer una "orden de compra" que notifica al Tenant. No requiere login por parte del cliente final.

**URL de acceso:**  
`https://www.mi-negociocr.com/tienda/{businessSlug}`  
_(el Tenant genera y comparte este link desde su dashboard)_

---

## 🏗️ Arquitectura general

El proyecto sigue **Clean Architecture** con las capas:

```
/Domain        → Entidades + reglas puras
/Application   → Casos de uso + DTOs + interfaces
/Infrastructure → EF Core + PostgreSQL (Supabase) + integraciones externas
/API           → Controllers (sin lógica de negocio)
```

**Stack:** .NET 8, ASP.NET Web API, EF Core, PostgreSQL, Railway (hosting)  
**Multi-tenant:** Toda entidad tiene `BusinessId`. Nunca cruzar datos entre negocios.

---

## 🗄️ Entidades existentes relevantes

> ⚠️ **NO crear nuevas tablas para estas entidades — ya existen en la DB.**

### `CatalogItem`
```
Id, BusinessId, CategoryId?, Name, Description?, Type (enum), 
HasVariants, BasePrice, TrackStock, IsActive, CreatedAt
→ Variants: ICollection<CatalogVariant>
→ Images: ICollection<CatalogImage>
→ Category: CatalogCategory?
```

### `CatalogVariant`
```
Id, CatalogItemId, SKU?, Price, CostPrice, ProfitMargin?,
StockQuantity, LowStockThreshold, IsActive, CreatedAt
→ VariantOptionValues: ICollection<CatalogVariantOptionValue>
→ VariantImages: ICollection<CatalogVariantImage>
```

### `CatalogImage`
```
Id, CatalogItemId, ImageUrl, IsPrimary
```

### `CatalogCategory`
```
Id, BusinessId, Name, Description?, IsActive, CreatedAt
```

### `Contact` ← **Se usará para guardar el cliente que compra**
```
Id, BusinessId, Name, Phone, Email?,
CreatedAt, LastActivityAt?, IsDeleted
```

### `Sale` ← **Se creará una venta desde la tienda**
```
Id, BusinessId, InvoiceNumber, Source,
Subtotal, DiscountAmount, TaxAmount, TotalOrden, Total,
ContactId?, CustomerPhone, SaleDate, CreatedAt
→ Items: ICollection<SaleItem>
→ PaymentMethods: ICollection<SalePaymentMethod>
```

### `SaleItem`
```
Id, SaleId, CatalogVariantId?, Description?, ItemType,
Quantity, Price, CostPrice, UnitPrice, Total
```

### `Business`
```
Id, Name, LogoUrl?, BusinessType?, Description?, Phone?,
Location?, PublicEmail?, WhatsappPhoneNumberId?,
EnableWhatsappNotifications, EnableEmailNotifications, IsActive
```

---

## 🆕 Nuevas entidades a crear

### `StoreOrder` _(tabla nueva)_
Representa la intención de compra desde la tienda pública, **antes** de que el Tenant la apruebe y convierta en `Sale`.

```csharp
public class StoreOrder
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }

    // Datos del cliente (se busca o crea Contact)
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }

    // Referencia al Contact (se crea/busca por Phone+BusinessId)
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    // Estado
    public StoreOrderStatus Status { get; set; } = StoreOrderStatus.Pending;

    // Nota opcional del cliente
    public string? CustomerNote { get; set; }

    // Totales calculados
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
    public Business Business { get; set; } = null!;
}
```

### `StoreOrderItem` _(tabla nueva)_
```csharp
public class StoreOrderItem
{
    public Guid Id { get; set; }
    public Guid StoreOrderId { get; set; }
    public Guid CatalogVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty; // snapshot nombre
    public string? VariantDescription { get; set; }         // snapshot opciones
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }                  // snapshot precio
    public decimal Subtotal { get; set; }

    public StoreOrder StoreOrder { get; set; } = null!;
    public CatalogVariant Variant { get; set; } = null!;
}
```

### `StoreOrderStatus` _(enum)_
```csharp
public enum StoreOrderStatus
{
    Pending = 0,    // Recién llegó, espera revisión del Tenant
    Reviewed = 1,   // El Tenant la vio
    Converted = 2,  // Se convirtió en Sale oficial
    Cancelled = 3   // Rechazada/cancelada
}
```

### `BusinessStoreSettings` _(tabla nueva, 1:1 con Business)_
Configuración de la tienda pública del Tenant.

```csharp
public class BusinessStoreSettings
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }

    // El slug único para la URL pública: /tienda/{slug}
    public string Slug { get; set; } = string.Empty;

    // Si la tienda está activa o no
    public bool IsStoreEnabled { get; set; } = true;

    // Mensaje de bienvenida personalizable
    public string? WelcomeMessage { get; set; }

    // Color principal (hex) para personalización básica de UI
    public string? BrandColor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Business Business { get; set; } = null!;
}
```

---

## 📦 Módulos de desarrollo

Dividir en módulos pequeños e independientes. Desarrollar y probar uno a la vez.

---

### ✅ Módulo 1 — Configuración de tienda (Tenant)
**Objetivo:** El Tenant puede activar su tienda y obtener su link único.

**Backend — nuevos endpoints (requieren `[Authorize]`):**

```
POST   api/store/settings          → Crear/actualizar configuración de tienda
GET    api/store/settings          → Obtener config de la tienda del Tenant
POST   api/store/settings/generate-slug → Generar slug único basado en nombre del negocio
```

**DTOs:**
```csharp
// Request
public record UpsertStoreSettingsRequest(
    bool IsStoreEnabled,
    string? WelcomeMessage,
    string? BrandColor,
    string? Slug  // opcional; si null se genera automáticamente
);

// Response
public record StoreSettingsResponse(
    Guid BusinessId,
    string Slug,
    bool IsStoreEnabled,
    string? WelcomeMessage,
    string? BrandColor,
    string PublicUrl  // "https://www.mi-negociocr.com/tienda/{slug}"
);
```

**Reglas de negocio:**
- El Slug debe ser único en todo el sistema (validar antes de guardar)
- Slug solo permite: letras minúsculas, números y guiones (`a-z0-9-`)
- Máximo 50 caracteres
- Generar slug automático desde `Business.Name` (slugify + sufijo corto si hay colisión)

**Migration EF:**
```
Add-Migration AddBusinessStoreSettings
```

---

### ✅ Módulo 2 — Catálogo público (sin auth)
**Objetivo:** Endpoints **públicos** para que el cliente final pueda ver el catálogo.

**Backend — nuevos endpoints (sin `[Authorize]`, solo `[AllowAnonymous]`):**

```
GET  api/public/store/{slug}                    → Info del negocio + config tienda
GET  api/public/store/{slug}/catalog            → Lista de productos activos
GET  api/public/store/{slug}/catalog/{itemId}   → Detalle de producto + variantes + imágenes
GET  api/public/store/{slug}/categories         → Categorías con productos activos
```

**DTOs de respuesta:**

```csharp
// Info del negocio (pantalla de bienvenida)
public record PublicStoreInfoResponse(
    string BusinessName,
    string? LogoUrl,
    string? Description,
    string? WelcomeMessage,
    string? BrandColor,
    string? Phone,
    string? Location
);

// Lista de productos
public record PublicCatalogItemSummary(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,         // precio mínimo/base para mostrar
    string? PrimaryImageUrl,
    string? CategoryName,
    bool HasVariants,
    bool InStock               // true si alguna variante activa tiene stock > 0
);

// Detalle de un producto
public record PublicCatalogItemDetail(
    Guid Id,
    string Name,
    string? Description,
    string? CategoryName,
    bool HasVariants,
    decimal BasePrice,
    List<string> Images,       // todas las URLs de imágenes
    List<PublicVariantInfo> Variants
);

public record PublicVariantInfo(
    Guid Id,
    decimal Price,
    bool InStock,
    int StockQuantity,         // solo si TrackStock = true; de lo contrario omitir
    List<string> Options       // ej: ["Color: Rojo", "Talla: M"]
);
```

**Reglas de negocio:**
- Solo mostrar `CatalogItem` donde `IsActive = true`
- Solo mostrar variantes donde `IsActive = true`
- No exponer `CostPrice`, `ProfitMargin`, datos internos
- Si `TrackStock = false`, `InStock = true` siempre
- Si `TrackStock = true`, `InStock = StockQuantity > 0`
- Si la tienda está desactivada (`IsStoreEnabled = false`), retornar `404` o mensaje apropiado

---

### ✅ Módulo 3 — Orden de compra pública (sin auth)
**Objetivo:** El cliente llena sus datos y confirma su pedido.

**Backend — nuevo endpoint público:**

```
POST  api/public/store/{slug}/orders   → Crear orden de compra
GET   api/public/store/{slug}/orders/{orderId}  → Consultar estado de su orden (opcional)
```

**DTO Request:**
```csharp
public record CreateStoreOrderRequest(
    string CustomerName,     // requerido
    string CustomerPhone,    // requerido (mínimo 8 dígitos)
    string? CustomerEmail,   // opcional
    string? CustomerNote,    // nota adicional del cliente
    List<StoreOrderItemRequest> Items
);

public record StoreOrderItemRequest(
    Guid VariantId,
    int Quantity             // mínimo 1
);
```

**DTO Response:**
```csharp
public record CreateStoreOrderResponse(
    Guid OrderId,
    string Message,          // "¡Tu pedido fue recibido! El negocio te contactará pronto."
    decimal Total
);
```

**Reglas de negocio:**
1. Validar que el `slug` existe y la tienda está activa
2. Validar que todos los `VariantId` pertenecen al negocio correcto (evitar IDOR)
3. Validar que `Quantity >= 1`
4. Si `CatalogItem.TrackStock = true`, verificar que `StockQuantity >= Quantity` (no descontar aún — solo validar disponibilidad)
5. **Buscar o crear Contact:**
   - Buscar `Contact` por `BusinessId + Phone` (no eliminado)
   - Si no existe → crear nuevo `Contact` con Name, Phone, Email
   - Si existe → actualizar `Name` y `Email` si vinieron en el request
6. Crear `StoreOrder` con `Status = Pending`, asociar `ContactId`
7. Calcular `Subtotal` y `Total` usando precios actuales de las variantes (snapshot)
8. Guardar `StoreOrderItem` con snapshot de nombre, precio, opciones
9. **Notificar al Tenant** (ver Módulo 4)
10. Retornar `CreateStoreOrderResponse`

---

### ✅ Módulo 4 — Notificaciones al Tenant
**Objetivo:** Cuando llega una orden nueva, avisar al dueño del negocio.

**Notificación por WhatsApp (si tiene WhatsApp configurado):**
```
Usar Business.WhatsappPhoneNumberId + WhatsappAccessToken
Mensaje: 
"🛒 *Nueva orden de tienda!*
👤 Cliente: {CustomerName}
📱 Teléfono: {CustomerPhone}
📧 Email: {CustomerEmail ?? "No indicado"}
📦 Productos: {lista de items}
💰 Total: ₡{Total:N0}
📝 Nota: {CustomerNote ?? "Ninguna"}"
```

**Notificación por Email (si tiene Email configurado):**
```
Usar SMTP configurado en Business
Asunto: "Nueva orden de tienda - {CustomerName}"
Cuerpo: HTML con los mismos datos
```

**Si ninguno está configurado:**
- Guardar la orden igual (el Tenant la verá en el dashboard)
- No lanzar error

---

### ✅ Módulo 5 — Gestión de órdenes (Tenant, requiere auth)
**Objetivo:** El Tenant puede ver y gestionar las órdenes recibidas.

**Backend — nuevos endpoints (requieren `[Authorize]`):**

```
GET     api/store/orders                    → Lista de órdenes (paginada, filtros)
GET     api/store/orders/{orderId}          → Detalle de orden
PATCH   api/store/orders/{orderId}/status   → Cambiar status (Reviewed, Cancelled)
POST    api/store/orders/{orderId}/convert  → Convertir en Sale oficial
```

**DTOs:**
```csharp
// Lista
public record StoreOrderSummary(
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    decimal Total,
    StoreOrderStatus Status,
    int ItemCount,
    DateTime CreatedAt
);

// Detalle
public record StoreOrderDetail(
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string? CustomerNote,
    StoreOrderStatus Status,
    decimal Subtotal,
    decimal Total,
    DateTime CreatedAt,
    List<StoreOrderItemDetail> Items
);

public record StoreOrderItemDetail(
    string ProductName,
    string? VariantDescription,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

// Cambio de status
public record UpdateStoreOrderStatusRequest(StoreOrderStatus NewStatus);
```

**Endpoint `convert` — lógica:**
- Crear un `Sale` normal usando los datos de la `StoreOrder`
- Usar `Source = "StoreOrder"`
- Los items se crean como `SaleItem` con los datos snapshot
- Si `TrackStock = true` → descontar stock en este momento
- Actualizar `StoreOrder.Status = Converted`
- Actualizar `Contact.LastActivityAt = DateTime.UtcNow`

**Filtros disponibles para listado:**
- `status`: pending | reviewed | converted | cancelled
- `dateFrom` / `dateTo`
- `search`: buscar por nombre o teléfono

---

## 🎨 Frontend — Pantallas móvil (diseño)

> ⚠️ El frontend es mobile-first. Todas las pantallas deben funcionar perfectamente en pantallas de 375px+. Usar el mismo stack/framework del proyecto actual.

### Pantalla 1 — Landing de tienda `/tienda/{slug}`
```
┌─────────────────────────┐
│  [Logo del negocio]     │
│  Nombre del negocio     │
│  Descripción breve      │
│                         │
│  "Bienvenido a nuestra  │
│   tienda digital"       │
│                         │
│  🔍 [Buscar productos]  │
│                         │
│  Categorías:            │
│  [Todas] [Cat1] [Cat2]  │
│                         │
│  ┌──────┐  ┌──────┐     │
│  │Foto  │  │Foto  │     │
│  │Prod1 │  │Prod2 │     │
│  │₡xxxx │  │₡xxxx │     │
│  └──────┘  └──────┘     │
│  ┌──────┐  ┌──────┐     │
│  │Foto  │  │Foto  │     │
│  ...                    │
└─────────────────────────┘
```

### Pantalla 2 — Detalle de producto
```
┌─────────────────────────┐
│  ← Volver               │
│                         │
│  [Imagen grande]        │
│  ○ ○ ● ○  (dots imgs)   │
│                         │
│  Nombre del producto    │
│  Descripción completa   │
│                         │
│  ₡ 12,500               │
│                         │
│  Color:                 │
│  [Rojo] [Azul] [Verde]  │
│                         │
│  Talla:                 │
│  [S] [M] [L]            │
│                         │
│  Cantidad: [-] 1 [+]    │
│                         │
│  [Agregar al carrito]   │
└─────────────────────────┘
```

### Pantalla 3 — Carrito
```
┌─────────────────────────┐
│  🛒 Tu pedido           │
│                         │
│  ┌───────────────────┐  │
│  │[img] Producto 1   │  │
│  │Azul / M           │  │
│  │₡12,500  x2  [🗑]  │  │
│  └───────────────────┘  │
│  ┌───────────────────┐  │
│  │[img] Producto 2   │  │
│  │...                │  │
│  └───────────────────┘  │
│                         │
│  Subtotal:  ₡25,000     │
│  Total:     ₡25,000     │
│                         │
│  [Continuar con pedido] │
└─────────────────────────┘
```

### Pantalla 4 — Datos del cliente (checkout)
```
┌─────────────────────────┐
│  📋 Tus datos           │
│                         │
│  Nombre completo *      │
│  [___________________]  │
│                         │
│  Teléfono *             │
│  [___________________]  │
│                         │
│  Email (opcional)       │
│  [___________________]  │
│                         │
│  Nota para el negocio   │
│  [___________________]  │
│  [___________________]  │
│                         │
│  Resumen: 3 productos   │
│  Total: ₡37,500         │
│                         │
│  [✓ Enviar pedido]      │
│                         │
│  🔒 Tus datos son       │
│  enviados al negocio    │
└─────────────────────────┘
```

### Pantalla 5 — Confirmación
```
┌─────────────────────────┐
│                         │
│        ✅               │
│                         │
│  ¡Pedido recibido!      │
│                         │
│  El negocio te          │
│  contactará pronto al   │
│  número indicado.       │
│                         │
│  📞 [Nombre negocio]    │
│  [teléfono negocio]     │
│                         │
│  [Seguir comprando]     │
│                         │
└─────────────────────────┘
```

---

## 🔧 Consideraciones técnicas

### Seguridad
- Los endpoints `/api/public/store/{slug}/*` son **completamente públicos** (`[AllowAnonymous]`)
- Validar siempre que `VariantId` pertenezca al negocio del slug (evitar IDOR)
- Rate limiting en `POST /orders` para evitar spam (máximo 5 órdenes por IP por hora recomendado)
- No exponer datos internos: sin `CostPrice`, sin `ProfitMargin`, sin tokens de WhatsApp

### Performance
- El catálogo puede cachearse en memoria por 5 minutos (productos cambian poco)
- Incluir `IsPrimary` para saber cuál imagen mostrar primero en la lista

### Slugs
- Al crear el slug, slugificar el nombre del negocio:
  - Minúsculas
  - Reemplazar espacios con `-`
  - Eliminar caracteres especiales
  - Si ya existe, agregar sufijo numérico: `mi-tienda-2`

### EF Core — Migrations necesarias
```
1. AddStoreOrderAndItems     → tablas StoreOrder + StoreOrderItem
2. AddBusinessStoreSettings  → tabla BusinessStoreSettings con índice único en Slug
```

### Índices DB recomendados
```sql
-- Para búsqueda de tienda por slug (muy frecuente)
CREATE UNIQUE INDEX idx_business_store_settings_slug 
  ON "BusinessStoreSettings" ("Slug");

-- Para listar órdenes del Tenant
CREATE INDEX idx_store_orders_business_status 
  ON "StoreOrders" ("BusinessId", "Status", "CreatedAt" DESC);
```

---

## 📋 Orden de implementación recomendada

| # | Módulo | Descripción | Deps |
|---|--------|-------------|------|
| 1 | **Entidades + Migration** | Crear `StoreOrder`, `StoreOrderItem`, `BusinessStoreSettings`, enum `StoreOrderStatus` + migrations EF | — |
| 2 | **Módulo 1** | Config de tienda (Tenant) | Módulo 1 |
| 3 | **Módulo 2** | Catálogo público | Módulo 1 |
| 4 | **Módulo 3** | Orden de compra pública | Módulo 2 |
| 5 | **Módulo 4** | Notificaciones al Tenant | Módulo 3 |
| 6 | **Módulo 5** | Gestión de órdenes (Tenant) | Módulos 3,4 |
| 7 | **Frontend** | Pantallas móvil (una por una) | Módulos 2,3 |

---

## ✅ Checklist por módulo

### Módulo 1 — Config tienda
- [ ] Entidad `BusinessStoreSettings` creada
- [ ] Migration aplicada
- [ ] Endpoint `POST api/store/settings` funcionando
- [ ] Endpoint `GET api/store/settings` funcionando
- [ ] Validación de slug único
- [ ] Slugify automático desde nombre del negocio

### Módulo 2 — Catálogo público
- [ ] Endpoint `GET api/public/store/{slug}` → info negocio
- [ ] Endpoint `GET api/public/store/{slug}/catalog` → lista productos
- [ ] Endpoint `GET api/public/store/{slug}/catalog/{itemId}` → detalle
- [ ] Solo productos activos con variantes activas
- [ ] Sin datos internos expuestos

### Módulo 3 — Orden pública
- [ ] Entidad `StoreOrder` + `StoreOrderItem` creadas
- [ ] Migration aplicada
- [ ] Endpoint `POST api/public/store/{slug}/orders`
- [ ] Buscar o crear `Contact` por Phone+BusinessId
- [ ] Validación de VariantId → pertenece al negocio
- [ ] Snapshot de precios al momento de la orden
- [ ] `Total` calculado correctamente

### Módulo 4 — Notificaciones
- [ ] WhatsApp notification cuando `EnableWhatsappNotifications = true`
- [ ] Email notification cuando `EnableEmailNotifications = true`
- [ ] Mensaje con todos los datos del pedido
- [ ] Falla silenciosa si no hay canal configurado

### Módulo 5 — Gestión Tenant
- [ ] Listado de órdenes con filtros
- [ ] Detalle de orden
- [ ] Cambio de status
- [ ] Conversión a `Sale` oficial
- [ ] Stock descontado al convertir (si `TrackStock = true`)

### Frontend
- [ ] Pantalla landing (grid de productos, búsqueda, filtro categoría)
- [ ] Pantalla detalle producto (galería, selector de variantes, cantidad)
- [ ] Pantalla carrito
- [ ] Pantalla checkout (formulario cliente)
- [ ] Pantalla confirmación
- [ ] Funciona en 375px (iPhone SE) sin scroll horizontal
- [ ] Loading states en cada petición
- [ ] Manejo de errores (tienda inactiva, producto sin stock, etc.)

---

## 🗣️ Notas finales para Cursor

- Seguir el patrón existente: `Controller → Use Case (Interface) → Implementation → Repository`
- Todos los nuevos use cases deben tener su interfaz en `/Application/Interfaces/UseCases/`
- Los nuevos repositorios en `/Application/Interfaces/Repositories/`
- Registrar todo en el DI container (buscar el archivo de extensiones de servicios existente)
- Los endpoints públicos van en un controller separado: `PublicStoreController.cs`
- Los endpoints del Tenant van en: `StoreController.cs`
- **Mobile first:** el frontend debe verse impecable en celular antes que en desktop
