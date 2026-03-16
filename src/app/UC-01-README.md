# UC-01 Ver conversaciones – Refactor aplicado

Estos archivos implementan el refactor UC-01 con tipos y mapeo de datos.

## Archivos creados

| Archivo | Descripción |
|--------|-------------|
| `core/config/api.config.ts` | URL base del API (por defecto `https://localhost:7176`). Usada por HTTP y SignalR. |
| `core/models/conversation.model.ts` | `ConversationDTO` (respuesta del API) y `Conversation` (modelo de UI). |
| `core/services/whatsapp.service.ts` | `getConversations()` tipado; usa `apiConfig.baseUrl` para no recibir 404. |
| `core/services/chat-signalr.service.ts` | SignalR con URL base del API (`/chatHub`); evita 404 en negotiate. |
| `layout/whatsapp-panel/whatsapp-panel.ts` | Componente con `loadConversations()` que mapea DTO → UI y usa mock si el API devuelve vacío o falla. |
| `layout/whatsapp-panel/whatsapp-panel.html` | Vista de lista y chat (referencia). |
| `layout/whatsapp-panel/whatsapp-panel.scss` | Estilos mínimos. |

## Cómo usarlos en tu proyecto Angular

1. Copia la carpeta `src/app/` (o solo estos archivos) a tu proyecto Angular real.
2. Asegura que `HttpClient` esté importado en el módulo que declara el componente o en `AppConfig` (provideHttpClient()).
3. Si tu Angular es &lt; 17, sustituye en el HTML `@if` / `@for` por `*ngIf` / `*ngFor`.
4. **URL del API**: En `core/config/api.config.ts` la base por defecto es `https://localhost:7176` (según `launchSettings.json`). Si el API corre en otro puerto, cambia `baseUrl` o define `window.__API_BASE_URL__` antes de arrancar la app.
5. **CORS**: El API tiene CORS habilitado para `http://localhost:4200` y `https://localhost:4200` con `AllowCredentials()` para SignalR.

## Contrato API

El backend devuelve (camelCase): `customerName`, `phoneNumber`, `lastMessage`, `lastMessageAt`, `unreadCount`.  
El componente mapea a: `name`, `phone`, `lastMessage`, `time`, `unread` y mantiene `id` (para `selectConversation` y SignalR).
