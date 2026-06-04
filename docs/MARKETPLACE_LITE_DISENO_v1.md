# Mi-NegocioCR Marketplace Lite — Diseño funcional v1

> Copia incluida en el repo API. Origen: `MARKETPLACE_LITE_DISENO_v1.md` en la raíz del workspace `Mi-negociocr/`.

## Objetivo

Permitir que cualquier negocio (Tenant) comparta un enlace público con sus clientes para que estos:

- Vean productos.
- Vean imágenes.
- Vean precios.
- Vean descripción.
- Realicen pedidos.
- El negocio reciba la solicitud inmediatamente.

No se procesarán pagos en línea en la versión inicial.

---

## Arquitectura general

### URL pública

Cada negocio tendrá una URL única.

Ejemplos:

```text
https://mi-negociocr.com/store/panaderia-san-jose
https://mi-negociocr.com/store/joyca-tech
https://mi-negociocr.com/store/mi-negocio
```

El slug será configurable por Tenant.

---

## Módulo 1 — Catálogo público

### Página principal

Mostrar:

- Logo del negocio
- Nombre
- Descripción
- WhatsApp
- Redes sociales
- Categorías

Diseño **Mobile First**.

### Productos

Card de producto:

- Imagen principal
- Nombre
- Precio
- Descripción corta

Botón:

```text
Ver Detalles
```

---

## Módulo 2 — Detalle de producto

Información:

- Galería de imágenes
- Nombre
- Precio
- Descripción completa
- Disponibilidad

Botón:

```text
Solicitar Producto
```

---

## Módulo 3 — Solicitud de compra

Formulario:

### Datos cliente

| Campo | Requerido |
|-------|-----------|
| Nombre completo | Sí |
| Teléfono | Sí |
| Email | No |
| Observaciones | No |

### Producto

- Cantidad

### Botón

```text
Enviar Solicitud
```

---

## Flujo de compra

Cliente presiona:

```text
Enviar Solicitud
```

Sistema:

1. Guarda cliente.
2. Guarda orden.
3. Notifica al negocio.
4. Muestra confirmación.

Mensaje:

```text
Gracias por su solicitud.

El negocio se pondrá en contacto con usted pronto.
```

---

## Módulo 4 — Clientes

Reutilizar tabla actual **Customers** (en MiNegocioCR: contactos CRM / `Contacts`).

Si existe teléfono:

- Actualizar información.

Si no existe:

- Crear cliente nuevo.

---

## Módulo 5 — Órdenes

Nueva tabla: **Orders**

Campos:

- Id
- TenantId
- CustomerId
- OrderDate
- Status
- Total
- Notes

### Tabla OrderItems

Campos:

- Id
- OrderId
- ProductId
- Quantity
- UnitPrice

### Estados

- Pending
- Contacted
- InProgress
- Completed
- Cancelled

---

## Módulo 6 — Notificaciones

Cuando entra una orden:

- Crear **Notification**.

Mensaje:

```text
Nueva solicitud recibida.

Cliente: Juan Pérez
Producto: Laptop Dell
Cantidad: 1
```

Mostrar en dashboard actual.

---

## Módulo 7 — Administración

Nueva opción menú: **Catálogo Digital**

Submenús:

- Productos
- Categorías
- Órdenes
- Configuración

---

## Módulo 8 — Configuración tienda

Campos:

- Nombre público
- Slug
- Descripción
- Logo
- Banner
- WhatsApp
- Facebook
- Instagram
- Color principal
- Color secundario

---

## Módulo 9 — SEO

Generar dinámicamente:

- Title
- Meta Description
- Open Graph

Para compartir en:

- WhatsApp
- Facebook
- Instagram

---

## Responsive design

Obligatorio:

- 320px
- 375px
- 390px
- 414px
- 768px
- 1024px

**Mobile First.**

---

## Performance

- Lazy loading imágenes.
- Compresión imágenes.
- Paginación.
- Máximo **20 productos por página**.

---

## Seguridad

- Nunca exponer `TenantId` en frontend.
- Todas las consultas filtradas por Tenant.
- Validar slug único.
- Rate limit para formularios públicos.
- Captcha opcional.

---

## Fase 1 — MVP

- Catálogo
- Detalle producto
- Solicitud compra
- Clientes
- Órdenes
- Notificaciones

---

## Fase 2

- Carrito
- Wishlist
- Cupones
- Promociones
- Productos destacados

---

## Fase 3

- Pago SINPE
- Pago tarjeta
- Tracking de órdenes
- Facturación automática

---

*Documento de diseño funcional v1 — Marketplace Lite · MiNegocioCR*
