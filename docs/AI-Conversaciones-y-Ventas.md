# Diseño: Conversaciones y flujo de ventas con IA

Documento para entender cómo está hecha la parte de conversaciones en el código y qué falta para lograr una **conversación continua donde la IA pueda vender** (pedir confirmación y registrar la venta).

---

## 1. Identificación de una conversación

Una conversación se identifica por:

- **BusinessId** (Guid del negocio)
- **PhoneNumber** (string del celular del cliente)

Mismo negocio + mismo número = misma conversación, sin importar si es "vieja" o nueva.

---

## 2. Dos capas de persistencia

### 2.1 Memoria de conversación (historial de mensajes)

| Qué | Tabla | Servicio | Uso |
|-----|--------|----------|-----|
| Mensajes user/assistant | `WhatsAppMessages` | `ConversationMemoryService` | Historial para contexto y continuidad |

- **SaveMessageAsync(businessId, phoneNumber, role, message)**  
  Guarda cada mensaje (user o assistant) con `BusinessId` + `PhoneNumber`.  
  Se llama al **inicio** de `AskAsync` (mensaje del usuario) y **después** de la respuesta de la IA (mensaje del assistant).

- **GetConversationContextAsync(businessId, phoneNumber, lastMessages)**  
  Devuelve los últimos N mensajes de esa conversación en texto (`"user: ..."`, `"assistant: ..."`).  
  Se usa para armar el prompt y que la IA tenga contexto de la conversación reciente.

**Estado actual:**  
✅ Mensajes se guardan.  
✅ El historial se incluye en el prompt ("Conversación reciente: …").

---

### 2.2 Estado de conversación (flujo de venta)

| Qué | Tabla | Servicio | Uso |
|-----|--------|----------|-----|
| Paso actual, producto elegido | `ConversationStates` | `ConversationStateService` | Flujo "esperando confirmación de compra" |

**Entidad `ConversationState`:**
- `BusinessId`, `PhoneNumber` (clave lógica)
- `Step` (ej. `"awaiting_confirmation"`)
- `ProductId` (Guid) → debe ser **CatalogVariant.Id** para registrar la venta
- `Price`, `UpdatedAt`

- **GetAsync(businessId, phoneNumber)**  
  Lee el estado actual. Se usa al inicio de `AskAsync` para saber si estamos en "esperando sí/no".

- **SaveAsync(state)**  
  Crea o actualiza el estado (por ejemplo paso `"awaiting_confirmation"` y producto elegido).  
  **Hoy no se llama en ningún sitio** → por eso nunca hay "conversación en modo venta".

- **ClearAsync(businessId, phoneNumber)**  
  Borra el estado. Se llama cuando el usuario responde "sí" (venta hecha) o "no" (cancelado).

**Estado actual:**  
✅ Se lee y se borra estado.  
❌ **Nunca se guarda estado** → la IA no puede pasar a "¿Confirmas la compra?" y luego registrar la venta en el siguiente mensaje.

---

## 3. Flujo actual en `AIService.AskAsync` (resumen)

```
1. Validar que el negocio exista.
2. Si EnableAIChat está desactivado → return "".
3. Guardar mensaje del usuario (SaveMessageAsync, role "user").
4. Leer estado de conversación (GetAsync).
5. Si hay estado y Step == "awaiting_confirmation":
   - Si mensaje contiene "sí" → CreateSaleAsync, ClearAsync, return resultado.
   - Si mensaje contiene "no" → ClearAsync, return "Compra cancelada.".
6. Guardrail (palabras permitidas); si no pasa → mensaje de rechazo.
7. Cache por (BusinessId, PhoneNumber, fecha, mensaje); si hay cache → return.
8. Clasificar intent (RepairOrder, Sales, Inventory) → elegir tool.
9. Ejecutar tool → toolData (Message, ProductId, ProductName, Price).
10. Si toolData.ProductId == null → intentar fallback con inventory_search.
11. Obtener historial (GetConversationContextAsync).
12. Construir prompt (historial + prompt de venta + data del tool + upsell).
13. Llamar IA → response.
14. Guardar mensaje del assistant (SaveMessageAsync, role "assistant").
15. Guardar cache y return response.
```

En ningún paso se llama a **SaveAsync** de `ConversationStateService`, por eso nunca se "entra" en el paso `awaiting_confirmation`.

---

## 4. Flujo deseado para "conversación continua que vende"

Para que la IA pueda:

1. Que el usuario diga "quiero el cargador" (o similar).
2. Que la IA responda algo como: "Tenemos Cargador universal por ₡X. ¿Confirmas la compra? Responde sí o no."
3. Que en el **siguiente** mensaje el usuario diga "sí" y se registre la venta,

hace falta que **antes de devolver la respuesta del paso 2** se guarde el estado de conversación con:

- `Step = "awaiting_confirmation"`
- `ProductId` = **CatalogVariant.Id** del producto que se está ofreciendo (no CatalogItem.Id)
- `Price` = precio mostrado
- Mismo `BusinessId` y `PhoneNumber` del request

Así, en el siguiente mensaje, en el paso 5 del flujo anterior, `GetAsync` devolverá ese estado y se procesará "sí"/"no" y se llamará a `CreateSaleAsync` con el `ProductId` correcto.

---

## 5. Dónde guardar el estado (recomendación)

**Lugar:** En `AIService`, después de tener la **response** de la IA y **antes** de `SaveMessageAsync` del assistant y del `return`.

**Condición para guardar estado:**  
Solo cuando tenga sentido "esperar un sí/no" para una venta, por ejemplo:

- Intent fue **Sales** (o el tool usado fue de ventas/inventario con un solo producto), **y**
- `toolData.ProductId` no es null (tenemos un producto concreto), **y**
- Opcional: la respuesta de la IA contiene frases de confirmación ("confirmas", "sí o no", etc.) o se decide por regla (ej. "si intent es Sales y hay un solo producto, siempre pedir confirmación").

**Qué guardar:**

- `ProductId` debe ser el **CatalogVariant.Id** (no CatalogItem.Id), porque `SaleService.CreateSaleAsync` recibe `variantId`.
- En el código actual, `InventoryTool` devuelve `ProductId = first.ItemId` (CatalogItemId). Para el flujo de venta hay que usar el **Id de la variante** (p. ej. `first.Id` en el tool) y que ese valor sea el que se guarde en `ConversationState.ProductId` y se pase a `CreateSaleAsync`.

**Ejemplo de llamada:**

```csharp
// Después de obtener response, si debemos pedir confirmación:
await _state.SaveAsync(new ConversationState
{
    Id = Guid.NewGuid(),
    BusinessId = request.BusinessId,
    PhoneNumber = request.PhoneNumber ?? "",
    ProductId = toolData.ProductId,   // debe ser Variant.Id
    Price = toolData.Price,
    Step = "awaiting_confirmation",
    UpdatedAt = DateTime.UtcNow
});
```

Solo hacer este `SaveAsync` cuando la lógica decida "en esta respuesta estamos pidiendo confirmación de compra para este producto".

---

## 6. Diagrama de flujo (resumen)

```
[Usuario envía mensaje]
        │
        ▼
┌───────────────────┐
│ Negocio existe?   │──No──► "Negocio no encontrado."
└─────────┬─────────┘
         │ Sí
         ▼
┌───────────────────┐
│ SaveMessageAsync  │  (user)
│ (memoria)         │
└─────────┬─────────┘
         │
         ▼
┌───────────────────┐     Sí    ┌─────────────────────┐
│ GetAsync (estado) │──────────►│ Step ==             │
└─────────┬─────────┘           │ "awaiting_confirmation"│
         │ No                  └──────────┬──────────┘
         │                                 │
         │                    ┌────────────┴────────────┐
         │                    │ "sí" → CreateSale,      │
         │                    │ ClearAsync, return      │
         │                    │ "no" → ClearAsync,      │
         │                    │ return "Cancelada"      │
         │                    └────────────────────────┘
         ▼
┌───────────────────┐
│ Cache?            │──Sí──► return cached
└─────────┬─────────┘
         │ No
         ▼
┌───────────────────┐
│ Intent → Tool     │
│ ExecuteAsync      │
└─────────┬─────────┘
         │
         ▼
┌───────────────────┐
│ GetConversation   │
│ ContextAsync      │
└─────────┬─────────┘
         │
         ▼
┌───────────────────┐
│ Prompt + IA       │
└─────────┬─────────┘
         │
         ▼
┌───────────────────┐
│ (FALTA)           │
│ SaveAsync estado  │  ← Solo si "pedimos confirmación"
│ Step=awaiting_    │     y ProductId = Variant.Id
│ confirmation      │
└─────────┬─────────┘
         │
         ▼
┌───────────────────┐
│ SaveMessageAsync  │  (assistant)
│ Cache, return     │
└───────────────────┘
```

---

## 7. Checklist para tener "conversación continua que vende"

1. **Guardar estado cuando la IA pide confirmación**  
   En `AIService`, después de la respuesta de la IA, si la lógica determina que se está pidiendo confirmación de compra, llamar a `_state.SaveAsync` con `Step = "awaiting_confirmation"` y `ProductId` = variante.

2. **Usar CatalogVariant.Id como ProductId**  
   En el tool que alimenta la venta (o en el resultado que se usa para "un solo producto"), exponer y usar el **Id de la variante** (no el de CatalogItem).  
   Ajustar `InventoryTool` (o el que sea) para que `ToolResult.ProductId` sea el Id de la variante cuando se quiera usar para venta.  
   Asegurar que ese mismo valor se guarde en `ConversationState.ProductId` y se pase a `SaleService.CreateSaleAsync`.

3. **Prompt claro**  
   Que el prompt indique a la IA que, cuando ofrezca un producto concreto para vender, pregunte explícitamente: "¿Confirmas la compra de [nombre] por ₡[precio]? Responde sí o no."

4. **Mismo BusinessId y PhoneNumber**  
   En todas las llamadas de la misma conversación usar el mismo `BusinessId` y `PhoneNumber` (normalizado si hace falta) para que GetAsync/SaveAsync/ClearAsync y la memoria coincidan.

Con esto, el flujo de "conversación continua para vender" queda definido y se puede implementar en el código tal como se describe arriba.
