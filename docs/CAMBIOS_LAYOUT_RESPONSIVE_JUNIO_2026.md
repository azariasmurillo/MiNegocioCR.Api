# Layout responsive — menú colapsable (Junio 2026)

**Repositorio:** `mi-negociocr-frontend` (`main`) — commit `d685a9b`  
**Tipo:** Solo frontend — sin cambios en API ni base de datos  
**Deploy:** Vercel (rebuild automático al push en `main`)  
**Copia workspace:** raíz `Mi-negociocr/CAMBIOS_LAYOUT_RESPONSIVE_JUNIO_2026.md`

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Qué incluye el release](#2-qué-incluye-el-release)
3. [Comportamiento por tamaño de pantalla](#3-comportamiento-por-tamaño-de-pantalla)
4. [Archivos tocados](#4-archivos-tocados)
5. [Pre-deploy checklist](#5-pre-deploy-checklist)
6. [Plan de pruebas manual](#6-plan-de-pruebas-manual)
7. [Mensaje de commit](#7-mensaje-de-commit)

---

## 1. Resumen ejecutivo

| Área | Entregable | Estado |
|------|------------|--------|
| Topbar | Botón para mostrar/ocultar menú lateral | ✅ |
| Desktop | Sidebar se oculta y el contenido usa todo el ancho | ✅ |
| Móvil (≤960px) | Menú como panel lateral con backdrop; cierra al navegar | ✅ |
| Preferencia | Estado guardado en `localStorage` (`mnr-sidebar-hidden`) | ✅ |
| Sidebar | Eliminada tarjeta decorativa «Workspace» para más espacio vertical | ✅ |

---

## 2. Qué incluye el release

### Menú colapsable

- Botón ☰ / `menu_open` en la barra superior (izquierda del título).
- Servicio `LayoutShellService` centraliza el estado con Angular signals.
- En pantallas pequeñas el menú inicia **oculto** si no hay preferencia guardada.
- Al cambiar de ruta en móvil, el menú se cierra automáticamente.
- Clic en el fondo semitransparente cierra el panel en móvil.

### Sidebar más compacto

- Removida la tarjeta **Workspace** («Tu negocio bajo control») entre el logo y la navegación.
- En móvil el menú abierto muestra textos completos (ya no el modo solo iconos de 96px).

### Topbar en pantallas chicas

- Subtítulo del topbar oculto en ≤640px.
- Título ligeramente más pequeño para ganar espacio horizontal.

---

## 3. Comportamiento por tamaño de pantalla

| Viewport | Menú cerrado | Menú abierto |
|----------|--------------|--------------|
| **>960px** | Sidebar oculto (`flex: 0`); contenido a ancho completo | Sidebar fijo 270px a la izquierda |
| **≤960px** | Sin sidebar visible; más espacio para tablas y formularios | Panel fijo ~280px + backdrop; tap fuera cierra |

**Persistencia:** `localStorage.setItem('mnr-sidebar-hidden', '1' | '0')`.

---

## 4. Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `src/app/layout/layout-shell.service.ts` | **Nuevo** — estado y persistencia del sidebar |
| `src/app/layout/layout-shell/layout-shell.ts` | Integración servicio; cierre en `NavigationEnd` (móvil) |
| `src/app/layout/layout-shell/layout-shell.html` | Clase `sidebar-hidden`, backdrop |
| `src/app/layout/layout-shell/layout-shell.scss` | Animaciones, panel fijo móvil, backdrop |
| `src/app/layout/topbar/topbar.ts` | Inyección `LayoutShellService` |
| `src/app/layout/topbar/topbar.html` | Botón toggle menú |
| `src/app/layout/topbar/topbar.scss` | `.topbar-leading`, responsive ≤640px |
| `src/app/layout/sidebar/sidebar.html` | Eliminada `.welcome-card` |
| `src/app/layout/sidebar/sidebar.scss` | Estilos welcome-card removidos; breakpoint ajustado |

---

## 5. Pre-deploy checklist

- [ ] `npm run build` sin errores TypeScript
- [ ] Probar toggle en desktop (Chrome, ancho >960px)
- [ ] Probar en DevTools móvil (≤960px): abrir menú, navegar, verificar cierre
- [ ] Verificar que la preferencia persiste al recargar la página
- [ ] Push a `main` → Vercel despliega automáticamente

**No requiere:** migraciones SQL, deploy Railway, variables de entorno nuevas.

---

## 6. Plan de pruebas manual

1. Login → dashboard con ventana ancha → clic en botón menú → sidebar desaparece → contenido ocupa todo el ancho.
2. Mismo estado → clic de nuevo → sidebar vuelve.
3. Reducir ventana a <960px → menú debe iniciar oculto (primera visita) o respetar `localStorage`.
4. Abrir menú → elegir «Ventas» → menú se cierra solo.
5. Abrir menú → clic en fondo oscuro → menú se cierra.
6. Recargar página → estado del menú se mantiene.
7. Confirmar que ya no aparece la tarjeta «Workspace» en el sidebar.

---

## 7. Mensaje de commit

```
feat(layout): menú lateral colapsable y sidebar más compacto

Botón en topbar para ocultar/mostrar el menú; panel drawer en móvil con
persistencia en localStorage. Se elimina la tarjeta Workspace del sidebar.
```

---

*Última actualización: 6 junio 2026*
