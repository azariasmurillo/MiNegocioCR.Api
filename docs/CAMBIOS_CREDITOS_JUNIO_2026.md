# Créditos / cuentas por cobrar — code review y deploy (Junio 2026)

**Workspace:** `c:\Mi-negociocr`  
**Repos Git separados:** `MiNegocioCR.Api` (`master`) · `mi-negociocr-frontend` (`main`)  
**Spec de producto:** [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./CREDITOS_CUENTAS_COBRAR_DISENO_v1.md)

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Fases implementadas](#2-fases-implementadas)
3. [Fixes importantes post-QA](#3-fixes-importantes-post-qa)
4. [Archivos por repositorio](#4-archivos-por-repositorio)
5. [Pre-deploy checklist](#5-pre-deploy-checklist)
6. [Base de datos](#6-base-de-datos)
7. [Plan de pruebas manual](#7-plan-de-pruebas-manual)
8. [Mensajes de commit sugeridos](#8-mensajes-de-commit-sugeridos)

---

## 1. Resumen ejecutivo

| Área | Entregable | Estado |
|------|------------|--------|
| API | Módulo `credit-accounts`: cargos, abonos, compromiso, comunicaciones, cancelar, dashboard KPIs | ✅ |
| BD | Migración `20260605163023_AddCreditAccounts` + script idempotente §9 | ✅ |
| Frontend | Ruta `/credits`, lista, detalle modal, POS crédito, print, export PDF/Excel | ✅ |
| Saldo | Replay desde historial (cargo + abono mismo timestamp) | ✅ |
| Fases 0–4 spec | Completas | ✅ |

---

## 2. Fases implementadas

| Fase | Contenido |
|------|-----------|
| **1** | EF, API, UI lista/detalle, cargos multi-línea, abonos, inventario, correo manual |
| **2** | POS «Registrar a crédito», KPIs dashboard, print estado de cuenta |
| **3** | Seguimiento (Llamada/WhatsApp/Visita/Otro), renovación compromiso en historial |
| **4** | Export PDF/Excel consolidado, agregar cargo a cuenta existente, archivar (saldo ₡0), tab **Archivadas** |

---

## 3. Fixes importantes post-QA

| Fix | Descripción |
|-----|-------------|
| Saldo tras abono | `CreditAccountBalanceResolver` + `effectiveCreditBalance` en FE |
| Fecha compromiso en detalle | Datepicker en MatDialog + `CreditCommitmentDateNormalizer` |
| Cargo + abono mismo momento | Saldo por **replay** del historial, no último snapshot |
| PDF lento | Config precargada; ventana de impresión síncrona al clic |
| Archivadas | Filtro API `archived` + tab UI |

---

## 4. Archivos por repositorio

### API (`MiNegocioCR.Api`)

| Área | Rutas clave |
|------|-------------|
| Entidades | `Domain/Entities/CreditAccount*.cs`, enums `Credit*` |
| Migración | `Migrations/20260605163023_AddCreditAccounts.cs` |
| Use cases | `Application/UseCases/CreditAccounts/*` |
| Helpers | `CreditAccountBalanceResolver`, `CreditChargeCalculator`, `CreditAccountProjection` |
| Controller | `API/Controllers/CreditAccountsController.cs` |
| Dashboard | `GetCreditDashboardSummaryUseCase`, `DashboardController` credits-summary |
| Script BD | `Scripts/apply-credit-accounts-migration.sql` (prod manual) · `apply-pending-migrations.sql` §9 |

### Frontend (`mi-negociocr-frontend`)

| Área | Rutas clave |
|------|-------------|
| Página | `features/credits/pages/credits/` |
| Detalle | `components/credit-account-detail-dialog.*` |
| Consolidar | `components/credit-add-charge-dialog.*` |
| Print | `pages/credit-account-print/`, `utils/credit-account-print-html.ts` |
| Export | `utils/credit-accounts-export.util.ts`, `credit-accounts-report-html.ts` |
| Saldo | `utils/credit-effective-balance.util.ts` |
| POS | `features/sales/pages/sales-manual/` (Registrar a crédito) |
| Dashboard | `features/dashboard/` panel créditos |

---

## 5. Pre-deploy checklist

- [ ] API detenida → `dotnet test` en `MiNegocioCR.Api`
- [ ] `npm run build` en `mi-negociocr-frontend`
- [ ] Migración aplicada en prod (EF o script §9)
- [ ] `Scripts/verify-schema.sql` sin faltantes (tablas Credit*)
- [ ] Smoke manual §7

---

## 6. Base de datos

### Preferido (local y prod con EF)

```powershell
cd c:\Mi-negociocr\MiNegocioCR.Api
dotnet ef database update
```

### Respaldo idempotente (Supabase / sin EF)

**Solo Créditos (recomendado en prod):**

```powershell
psql "<POSTGRES_CONNECTION_STRING>" -f Scripts/apply-credit-accounts-migration.sql
```

**Todas las migraciones pendientes (incluye §9 Créditos):**

```powershell
psql "<POSTGRES_CONNECTION_STRING>" -f Scripts/apply-pending-migrations.sql
```

Registra migración: `20260605163023_AddCreditAccounts`

Tablas nuevas: `CreditAccounts`, `CreditTransactions`, `CreditTransactionLines`, `CreditCommunications`

---

## 7. Plan de pruebas manual

| # | Caso | Resultado esperado |
|---|------|-------------------|
| 1 | Nuevo cargo (cliente nuevo) | Cuenta auto-creada, stock descontado si inventario |
| 2 | Abono parcial | Saldo arriba e historial actualizados |
| 3 | Abono > saldo | Saldo ₡0 + vuelto registrado |
| 4 | Agregar cargo a cuenta existente | Saldo suma; no duplica cuenta |
| 5 | Cargo + abono seguidos | Saldo = replay (ej. 22131.50, no 500) |
| 6 | Fecha compromiso sin fecha inicial | Guardar en detalle persiste |
| 7 | Seguimiento Llamada/WhatsApp | Aparece en historial |
| 8 | Cambiar fecha compromiso | Entrada Renovación en historial |
| 9 | Imprimir / Enviar correo | HTML con logo negocio |
| 10 | Export PDF / Excel lista | Reporte con totales |
| 11 | Archivar cuenta (saldo ₡0) | Desaparece de Activos; visible en **Archivadas** |
| 12 | POS «Registrar a crédito» | Cargo en cuenta del cliente |
| 13 | Dashboard KPIs créditos | Totales coherentes con lista |

---

## 8. Mensajes de commit sugeridos

**API:**

```
feat(credits): módulo cuentas por cobrar fases 1–4

Cargos multi-línea, abonos, comunicaciones, archivar, saldo por replay del historial.
Migración AddCreditAccounts y script apply-pending-migrations §9.
```

**Frontend:**

```
feat(credits): UI créditos, POS, export, archivadas

Lista/detalle, consolidar cargo, print, PDF/Excel, tab Archivadas, fix saldo UI.
```

---

*Última actualización: 5 junio 2026*
