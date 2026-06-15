# ZIP de prueba — import de fotos por SKU

## 1. Generar el ZIP con tus SKUs reales

En Inventario, abrí un producto y anotá el **SKU de la variante** (exacto, mayúsculas/minúsculas da igual al matchear).

Desde la raíz del repo API:

```powershell
cd MiNegocioCR.Api
.\Scripts\create-variant-import-test-zip.ps1 -Skus "TU-SKU-1","TU-SKU-2"
```

Salida: `Scripts/test-data/variant-image-import/variant-images-test-TU-SKU-1.zip`

Cada SKU genera `{SKU}_1.jpg` y `{SKU}_2.jpg` (fondos de prueba con círculo de color).

## 2. Dónde probar (UI)

1. API en marcha (`F5` en Visual Studio → `https://localhost:7176`)
2. Frontend (`ng serve` → `http://localhost:4200`)
3. Login con un usuario que tenga **businessId** en el JWT
4. Menú **Inventario**
5. Botón **Importar fotos ZIP** (arriba, junto a «+ Agregar»)
6. Elegí el `.zip`, opcional «Reemplazar imágenes existentes», **Importar**
7. Esperá el progreso y revisá la tabla de detalle

## 3. Probar por Swagger (opcional)

1. `https://localhost:7176/swagger`
2. Authorize con `Bearer {tu JWT}`
3. `POST /api/catalog/variant-images/import-zip` → multipart, campo `file` = el ZIP
4. Respuesta **202** `{ "batchId": "..." }`
5. `GET .../import-batches/{batchId}` y `.../logs` para el reporte

## 4. Requisitos previos

- Migraciones aplicadas (`AddUniqueSkuPerBusiness`, `AddVariantImageImport`)
- **Supabase** configurado en `appsettings.Development.json` (subida de WebP)
- Variantes con SKU único por negocio; si el SKU no existe → log `VariantNotFound`

## 5. Nombres válidos en el ZIP

`{SKU}_1.jpg` … `_3` · extensiones: jpg, jpeg, png, webp · máx. 3 fotos por variante
