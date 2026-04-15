# Handoff: APIs de Inventario y Catálogo (Backend)

Documento para compartir con el equipo/UI en otro repositorio. Resume el estado actual de las APIs necesarias para construir la pantalla/flujo de inventarios y variantes.

---

## Objetivo del UI

Construir interfaz para:

- crear productos de catálogo,
- crear categorías,
- crear opciones (ej. Color, Tamaño),
- crear valores por opción (ej. Negro, 16GB),
- crear variantes con combinación de `optionValueIds`,
- ajustar stock manualmente,
- registrar compras/ventas que impactan inventario.

---

## Endpoints disponibles

## 1) Catálogo base

### POST `/api/catalog`
Crea un producto base.

Request:

```json
{
  "businessId": "GUID",
  "name": "iPhone 15",
  "basePrice": 699.99,
  "trackStock": true,
  "type": 0
}
```

Response:
- `200 OK` + `Guid` (id del catalog item)

---

## 2) Categorías

### POST `/api/categories`

Request:

```json
{
  "businessId": "GUID",
  "name": "Celulares"
}
```

Response:
- `200 OK` + `Guid` (id categoría)

### GET `/api/categories/{businessId}`

Response:

```json
[
  {
    "id": "GUID",
    "businessId": "GUID",
    "name": "Celulares",
    "isActive": true,
    "createdAt": "2026-04-14T..."
  }
]
```

---

## 3) Opciones por producto

### POST `/api/options`

Request:

```json
{
  "catalogItemId": "GUID",
  "name": "Color"
}
```

Response:
- `200 OK` + `Guid` (id opción)

### GET `/api/options/{catalogItemId}`

Response:

```json
[
  {
    "id": "GUID",
    "catalogItemId": "GUID",
    "name": "Color"
  }
]
```

---

## 4) Valores por opción

### POST `/api/option-values`

Request:

```json
{
  "optionId": "GUID",
  "value": "Negro"
}
```

Response:
- `200 OK` + `Guid` (id option value)

### GET `/api/option-values/{optionId}`

Response:

```json
[
  {
    "id": "GUID",
    "optionId": "GUID",
    "value": "Negro"
  }
]
```

---

## 5) Variantes con combinación de option values

### POST `/api/variants`

Request:

```json
{
  "catalogItemId": "GUID",
  "sku": "IP15-NEG-128",
  "price": 799.99,
  "initialStock": 10,
  "optionValueIds": ["GUID_NEGRO", "GUID_128GB"]
}
```

Response:
- `200 OK` + `Guid` (id variante)

### Reglas de negocio vigentes

- `optionValueIds` puede ir vacío.
- Si se envían `optionValueIds`:
  - no permite ids duplicados dentro del request,
  - todos los option values deben pertenecer al mismo `catalogItemId`,
  - no permite combinación repetida exacta para ese item.

### Nota de persistencia

- La relación de combinación se guarda en tabla `CatalogVariantOptionValues`.

---

## 6) Ajuste manual de inventario

### POST `/api/inventory/adjust`

Request:

```json
{
  "businessId": "GUID",
  "variantId": "GUID",
  "adjustment": 5,
  "reason": "Ingreso manual"
}
```

Response:
- `200 OK`

Regla:
- `adjustment` debe ser distinto de 0.

---

## 7) Movimientos automáticos de inventario

## Compras (suben stock)

### POST `/api/purchases`

Request:

```json
{
  "businessId": "GUID",
  "items": [
    {
      "variantId": "GUID",
      "quantity": 3,
      "cost": 500.0
    }
  ]
}
```

Response:
- `200 OK`

## Ventas (bajan stock)

### POST `/api/sales`

Request:

```json
{
  "businessId": "GUID",
  "customerPhone": "5068XXXXXXX",
  "items": [
    {
      "variantId": "GUID",
      "quantity": 1,
      "price": 899.99
    }
  ]
}
```

Response:
- `200 OK` + `Guid` (id venta)

---

## Checklist sugerido para UI

- Crear pantallas/formularios para:
  - catálogo base,
  - categorías,
  - opciones,
  - valores de opción,
  - variantes con selector múltiple de `optionValueIds`.
- Incluir validación de frontend:
  - campos requeridos,
  - GUIDs válidos,
  - evitar enviar duplicados en `optionValueIds`.
- Manejar errores backend (`400`, `404`) y mostrar mensaje claro al usuario.
- En variante: prevenir combinación repetida desde UI (si ya cargaste combinaciones existentes), aunque backend ya lo valida.

---

## Nota operativa de despliegue

Si en algún ambiente aparece este error al crear variantes con opciones:

`42P01: relation "CatalogVariantOptionValues" does not exist`

falta aplicar migraciones de EF Core en backend:

```bash
dotnet ef database update --project MiNegocioCR.Api.csproj
```

---

## Resumen

El backend ya soporta el flujo completo de inventario + variantes por combinación de opciones. La UI puede proceder a consumir estos endpoints para habilitar gestión funcional de productos, variantes y stock.
