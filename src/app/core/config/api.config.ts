/**
 * URL base del API. Cambia según entorno (dev/prod).
 * Si Angular corre en localhost:4200, aquí debe ir la URL donde corre el API
 * (ej. https://localhost:5001 o http://localhost:5000).
 */
export const apiConfig = {
  baseUrl: (typeof (window as any).__API_BASE_URL__ === 'string'
    ? (window as any).__API_BASE_URL__
    : null) ?? 'https://localhost:7176',
};
