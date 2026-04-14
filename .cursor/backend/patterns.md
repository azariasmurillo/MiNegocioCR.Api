# Backend Patterns – Mi-NegocioCR (.NET)

## 🔌 Repository Pattern

### Regla
- Todo acceso a base de datos debe hacerse mediante repositorios
- Nunca acceder directamente desde controllers o services a DbContext

### Ejemplo

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
}