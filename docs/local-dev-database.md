# Base de datos local (PostgreSQL)

## Setup inicial

```bash
dotnet ef database update
psql -U postgres -d MiNegocioCR_Dev -f scripts/apply-pending-migrations.sql
```

## Por qué hace falta el script SQL

Existen dos migraciones (`AddSaleCostAndProfitMetrics`, `RefactorPaymentsAndSalePaymentMethods`) que no tienen archivo `.Designer.cs`, por lo que `dotnet ef database update` **no las aplica** aunque el código ya las necesita.

Sin esas tablas/columnas, el dashboard y ventas responden **500** (no es por falta de datos; con BD vacía deberían devolver ceros/listas vacías).

## Verificar schema

```sql
SELECT column_name FROM information_schema.columns
WHERE table_name = 'Sales' AND column_name IN ('TotalProfit', 'TotalOrden', 'PrepaidAmount');
```

Deben existir las tres columnas y la tabla `SalePaymentMethods`.
