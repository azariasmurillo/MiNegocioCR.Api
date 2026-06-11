# MiNegocioCR — Documentación del workspace

Monorepo local: **API** (`MiNegocioCR.Api/`) + **Frontend** (`mi-negociocr-frontend/`).  
Cada carpeta tiene su **propio repositorio Git** (API → `master`, frontend → `main`).

| Proyecto | README |
|----------|--------|
| API (.NET 8) | [../README.md](../README.md) |
| Frontend (Angular 21) | Repo [mi-negociocr-frontend](https://github.com/azariasmurillo/mi-negociocr-frontend) |

> **Nota:** Este índice vive en `MiNegocioCR.Api/docs/` para versionarlo en Git. La carpeta padre `Mi-negociocr/` no es un repositorio Git.

---

## Documentos en este repo (`docs/`)

| Documento | Para qué sirve |
|-----------|----------------|
| [DEPLOY.md](./DEPLOY.md) | Guía completa de deploy (Railway + Vercel + Supabase) |
| [FIXES_MAYO_2026.md](./FIXES_MAYO_2026.md) | Changelog vivo de correcciones (bugs, UI, BD) |
| [SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md](./SETUP_LOCAL_Y_CAMBIOS_MAYO_2026.md) | PostgreSQL local, Resend, tenant JoyCaTech, notas operativas |
| [email-campaigns-crm.md](./email-campaigns-crm.md) | Campañas de correo CRM (cola, límites, emergencia) |
| [CAMBIOS_PEDIDOS_INTERNET_JUNIO_2026.md](./CAMBIOS_PEDIDOS_INTERNET_JUNIO_2026.md) | **Pedidos Internet** — code review y checklist |
| [CAMBIOS_CREDITOS_JUNIO_2026.md](./CAMBIOS_CREDITOS_JUNIO_2026.md) | **Créditos** — code review y deploy |
| [CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md](./CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md) | **Layout responsive** — menú colapsable (solo frontend) |
| [CAMBIOS_REPARACIONES_JUNIO_2026.md](./CAMBIOS_REPARACIONES_JUNIO_2026.md) | **Reparaciones UX** — modal crear orden, búsqueda auto, Impresora (solo frontend) |
| [INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md) | **Inventario UX** — diseño aprobado + sprints 1–4 |
| [CAMBIOS_INVENTARIO_API_JUNIO_2026.md](./CAMBIOS_INVENTARIO_API_JUNIO_2026.md) | Inventario API — toggle presentación, márgenes, fixes |
| [INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md](./INVENTARIO_UX_ESPECIFICACION_PARA_REDISENO_v1.md) | Inventario UX — análisis estado actual (v1) |
| [PEDIDOS_INTERNET_DISENO_v1.md](./PEDIDOS_INTERNET_DISENO_v1.md) | Spec funcional Pedidos Internet (**implementado**) |
| [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./CREDITOS_CUENTAS_COBRAR_DISENO_v1.md) | Créditos / cuentas por cobrar — spec v1.1 (**implementado**) |
| [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md) | **Tienda digital — spec oficial** |
| [MARKETPLACE_LITE_DISENO_v1.md](./MARKETPLACE_LITE_DISENO_v1.md) | Visión producto Chat (referencia) |

### Módulos del producto

| Módulo | Estado | Spec / changelog |
|--------|--------|------------------|
| Inventario, ventas, reparaciones, CRM, dashboard | Producción | `FIXES_MAYO_2026.md` |
| Pedidos Internet | Producción (jun 2026) | [PEDIDOS_INTERNET_DISENO_v1.md](./PEDIDOS_INTERNET_DISENO_v1.md) |
| Créditos y cuentas por cobrar | Producción (jun 2026) | [CREDITOS_CUENTAS_COBRAR_DISENO_v1.md](./CREDITOS_CUENTAS_COBRAR_DISENO_v1.md) |
| Layout responsive (menú colapsable) | Producción (jun 2026) | [CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md](./CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md) |
| Inventario UX (rediseño) | Producción (jun 2026) | [INVENTARIO_UX_REDISENO_v2.md](./INVENTARIO_UX_REDISENO_v2.md) · FE: `mi-negociocr-frontend/docs/CAMBIOS_INVENTARIO_SPRINT4_JUNIO_2026.md` |
| Reparaciones UX | Listo para deploy (jun 2026) | [CAMBIOS_REPARACIONES_JUNIO_2026.md](./CAMBIOS_REPARACIONES_JUNIO_2026.md) |
| Tienda digital pública | Backlog | [TIENDA_DIGITAL_DISENO_UNIFICADO.md](./TIENDA_DIGITAL_DISENO_UNIFICADO.md) |

---

## Scripts SQL

| Script | Uso |
|--------|-----|
| `Scripts/apply-pending-migrations.sql` | Migraciones idempotentes (prod/local) |
| `Scripts/apply-credit-accounts-migration.sql` | Créditos — migración dedicada |
| `Scripts/verify-schema.sql` | Verificar columnas críticas post-deploy |

---

## Comandos rápidos pre-deploy

```powershell
cd MiNegocioCR.Api
dotnet test

cd ../mi-negociocr-frontend
npm run build
```

---

*Última actualización: 7 junio 2026*
