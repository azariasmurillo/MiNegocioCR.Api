# Backend Architecture – Mi-NegocioCR (.NET)

## 🧠 Stack

- .NET 8
- ASP.NET Web API
- Entity Framework Core
- PostgreSQL (Supabase)
- SignalR (opcional)
- Hosting: Railway

---

## 🏗️ Arquitectura

Se utiliza Clean Architecture:

/Domain
/Application
/Infrastructure
/API

---

## 🔀 Capas

### Domain
- Entidades
- Value Objects
- Interfaces (contratos)
- Reglas de negocio puras

❌ NO depende de nada

---

### Application
- Casos de uso
- Servicios de aplicación
- DTOs
- Interfaces de repositorio

✔ Depende de Domain

---

### Infrastructure
- Implementaciones
- EF Core
- Integraciones externas (WhatsApp, Email)

✔ Implementa interfaces de Application

---

### API
- Controllers
- Middlewares
- Validaciones básicas

❌ NO lógica de negocio

---

## 🔗 Flujo

Controller → Application → Domain → Infrastructure → DB

---

## 🧠 Multi-tenant

- Todas las entidades deben incluir BusinessId
- Todas las queries deben filtrar por BusinessId
- Nunca exponer datos cruzados entre negocios

---

## ⚠️ Reglas arquitectónicas

- Domain no depende de Infrastructure
- Controllers no contienen lógica
- Servicios usan interfaces (DI)

---

## 📋 Próximo módulo (documentado)

**Créditos / cuentas por cobrar** — spec `docs/CREDITOS_CUENTAS_COBRAR_DISENO_v1.md` (sin entidades EF ni controllers aún). Patrón esperado: UseCases + Controller como `InternetOrders` / `RepairOrders`, filtro `BusinessId`.

---

## 🎯 Objetivo

Backend escalable, desacoplado y mantenible