# Backend Rules – Mi-NegocioCR

## 🧩 Principios SOLID

### Single Responsibility
- Cada servicio hace una sola cosa

### Open/Closed
- Extender sin modificar código existente

### Dependency Inversion
- Usar interfaces + DI siempre

---

## ⚠️ Defensive Programming

### Validaciones

- Validar inputs en Application layer
- Validar nulls siempre

```csharp
if (request == null)
    throw new ArgumentNullException(nameof(request));
```