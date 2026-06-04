# Tienda digital pública — Diseño unificado (MiNegocioCR)

> **Spec oficial para desarrollo.** Copia en repo API. Origen: `TIENDA_DIGITAL_DISENO_UNIFICADO.md` en la raíz del workspace `Mi-negociocr/`.

| Decisión | Valor acordado |
|----------|----------------|
| URL pública | `https://www.mi-negociocr.com/tienda/{slug}` |
| Pedidos | `StoreOrder` + `StoreOrderItem` |
| Ítems | `CatalogVariantId` + snapshot |
| Clientes | `Contacts` |
| Cierre | `Pending` → `Reviewed` → `Converted` (`Sale`, `Source = "StoreOrder"`) |
| MVP | **Carrito multi-ítem** + un `POST` al confirmar |
| Pagos online | No en MVP |

Ver documento completo en el workspace (mismas secciones). Resumen de implementación:

## Orden

1. Entidades + migración  
2. Config tienda (tenant)  
3. API pública catálogo  
4. FE: landing, detalle, carrito (`sessionStorage`), checkout  
5. POST orden + Contact  
6. Notificaciones WA/email  
7. Gestión órdenes + convert to Sale  

## Controllers

- `PublicStoreController` — `[AllowAnonymous]`  
- `StoreController` — `[Authorize]`  

## Referencias

- [TIENDA_DIGITAL_SPEC.md](../../TIENDA_DIGITAL_SPEC.md) — detalle DTOs  
- [MARKETPLACE_LITE_DISENO_v1.md](./MARKETPLACE_LITE_DISENO_v1.md) — visión producto  

*Última actualización: mayo 2026*
