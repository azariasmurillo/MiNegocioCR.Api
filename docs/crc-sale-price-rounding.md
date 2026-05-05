# Redondeo de precios de venta en CRC (colones)

## Regla de negocio

Los precios de venta en colones que persistimos para variantes de catálogo siguen la misma política que el frontend (`normalizeColonesSaleUnitPrice` / `variant-sale-price.ts`):

1. Redondeo monetario a **2 decimales** (`MidpointRounding.AwayFromZero`).
2. Si el resultado es **≤ 0**, el precio es **0**.
3. Si ya es **múltiplo de 5**, se devuelve tal cual (tras el paso 1).
4. Si no, **siempre hacia arriba** al siguiente múltiplo de 5: `Ceiling(n / 5) × 5`.

No usar redondeo al entero más cercano ni floor hacia múltiplo de 5: solo **ceil** al paso 5.

## Implementación en el API

- **Dominio:** `MiNegocioCR.Api.Domain.Pricing.CrcSalePriceNormalizer.NormalizeSalePriceColones`.
- **Persistencia variante:** `CatalogVariantPriceResolver.ResolvePersistedPrice` aplica la normalización al valor final (manual, desde costo + margen, o fallback).
- **Lectura listados:** `GetVariantsByCatalogItemUseCase` y `GetVariantsByBusinessUseCase` proyectan `price` normalizado para datos legacy que aún no pasaron por migración.

## Migración de datos existentes

Script opcional: `Scripts/normalize_catalog_variant_prices_crc.sql` — ejecutar en staging con revisión previa.

## Alcance actual

Se aplicó a **precio de venta de variantes (`CatalogVariant.Price`)**. Totales de ventas (`Sale` / líneas) quedan fuera hasta alinear explícitamente con el POS (`computeManualSaleTotals`).
