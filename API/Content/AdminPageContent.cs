namespace MiNegocioCR.Api.API.Content;

public static class AdminPageContent
{
    public static string LoginHtml => """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Admin - Mi-NegocioCR</title>
            <style>
                * { box-sizing: border-box; }
                body { font-family: system-ui, -apple-system, sans-serif; margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #0f172a; color: #e2e8f0; }
                .card { background: #1e293b; padding: 2rem; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.3); width: 100%; max-width: 360px; }
                h1 { margin: 0 0 1.5rem; font-size: 1.5rem; color: #f8fafc; }
                label { display: block; margin-bottom: 0.5rem; font-size: 0.9rem; color: #94a3b8; }
                input { width: 100%; padding: 0.75rem; border: 1px solid #334155; border-radius: 8px; background: #0f172a; color: #e2e8f0; font-size: 1rem; margin-bottom: 1rem; }
                input:focus { outline: none; border-color: #3b82f6; }
                button { width: 100%; padding: 0.75rem; background: #2563eb; color: white; border: none; border-radius: 8px; font-size: 1rem; cursor: pointer; }
                button:hover { background: #1d4ed8; }
                .error { color: #f87171; font-size: 0.9rem; margin-top: 0.5rem; }
            </style>
        </head>
        <body>
            <div class="card">
                <h1>Acceso administrador</h1>
                <form id="loginForm">
                    <label for="password">Contraseña</label>
                    <input type="password" id="password" name="password" required autocomplete="current-password">
                    <div id="error" class="error" style="display:none;"></div>
                    <button type="submit">Entrar</button>
                </form>
            </div>
            <script>
                document.getElementById('loginForm').onsubmit = async (e) => {
                    e.preventDefault();
                    const err = document.getElementById('error');
                    err.style.display = 'none';
                    const res = await fetch('/api/admin/login', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ password: document.getElementById('password').value })
                    });
                    if (res.ok) {
                        window.location.href = '/admin/dashboard';
                        return;
                    }
                    const data = await res.json().catch(() => ({}));
                    err.textContent = data.error || 'Contraseña incorrecta';
                    err.style.display = 'block';
                };
            </script>
        </body>
        </html>
        """;

    public static string DashboardHtml => """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Admin Negocios - Mi-NegocioCR</title>
            <style>
                * { box-sizing: border-box; }
                body { font-family: system-ui, -apple-system, sans-serif; margin: 0; background: #0f172a; color: #e2e8f0; padding: 1.5rem; }
                a { color: #60a5fa; }
                h1 { margin: 0 0 1.5rem; font-size: 1.5rem; }
                .section { background: #1e293b; padding: 1.5rem; border-radius: 12px; margin-bottom: 1.5rem; max-width: 900px; }
                .section h2 { margin: 0 0 1rem; font-size: 1.1rem; color: #94a3b8; }
                input[type="text"] { padding: 0.6rem; border: 1px solid #334155; border-radius: 8px; background: #0f172a; color: #e2e8f0; width: 100%; max-width: 320px; margin-right: 0.5rem; margin-bottom: 0.5rem; }
                button { padding: 0.6rem 1rem; border-radius: 8px; border: none; cursor: pointer; font-size: 0.9rem; }
                .btn-primary { background: #2563eb; color: white; }
                .btn-primary:hover { background: #1d4ed8; }
                .btn-danger { background: #dc2626; color: white; }
                .btn-danger:hover { background: #b91c1c; }
                .btn-success { background: #16a34a; color: white; }
                table { width: 100%; border-collapse: collapse; }
                th, td { text-align: left; padding: 0.75rem; border-bottom: 1px solid #334155; }
                th { color: #94a3b8; font-weight: 600; }
                .badge { display: inline-block; padding: 0.25rem 0.5rem; border-radius: 6px; font-size: 0.8rem; }
                .badge-active { background: #166534; color: #86efac; }
                .badge-inactive { background: #991b1b; color: #fca5a5; }
                .loading { color: #94a3b8; }
                .msg { margin-top: 0.5rem; font-size: 0.9rem; }
                .msg.ok { color: #86efac; }
                .msg.err { color: #f87171; }
            </style>
        </head>
        <body>
            <h1>Administración de negocios</h1>
            <div class="section">
                <h2>Crear negocio</h2>
                <input type="text" id="newName" placeholder="Nombre del negocio">
                <button type="button" class="btn-primary" id="btnCreate">Crear</button>
                <div id="createMsg" class="msg"></div>
            </div>
            <div class="section">
                <h2>Negocios en el sistema</h2>
                <div id="listLoading" class="loading">Cargando...</div>
                <table id="businessTable" style="display:none;">
                    <thead><tr><th>Nombre</th><th>Estado</th><th>Creado</th><th>Acciones</th></tr></thead>
                    <tbody id="businessBody"></tbody>
                </table>
            </div>
            <p><a href="/admin/logout">Cerrar sesión</a> (volver al login)</p>
            <script>
                const api = (path, opts = {}) => fetch(path, { credentials: 'same-origin', ...opts });
                async function loadBusinesses() {
                    const res = await api('/api/admin/businesses');
                    if (res.status === 401) { window.location.href = '/admin'; return []; }
                    if (!res.ok) return [];
                    return res.json();
                }
                function renderList(items) {
                    document.getElementById('listLoading').style.display = 'none';
                    const table = document.getElementById('businessTable');
                    const tbody = document.getElementById('businessBody');
                    tbody.innerHTML = '';
                    if (items.length === 0) {
                        tbody.innerHTML = '<tr><td colspan="4">No hay negocios.</td></tr>';
                    } else {
                        items.forEach(b => {
                            const tr = document.createElement('tr');
                            const created = b.createdAt ? new Date(b.createdAt).toLocaleDateString('es') : '-';
                            tr.innerHTML = '<td>' + escapeHtml(b.name) + '</td><td><span class="badge ' + (b.isActive ? 'badge-active">Activo' : 'badge-inactive">Inactivo') + '</span></td><td>' + created + '</td><td>' +
                                (b.isActive ? '<button type="button" class="btn-danger" data-id="' + b.id + '" data-action="deactivate">Desactivar</button>' : '<button type="button" class="btn-success" data-id="' + b.id + '" data-action="activate">Activar</button>') + '</td>';
                            tbody.appendChild(tr);
                        });
                    }
                    table.style.display = 'table';
                    tbody.querySelectorAll('[data-action]').forEach(btn => {
                        btn.addEventListener('click', () => setStatus(btn.dataset.id, btn.dataset.action === 'activate'));
                    });
                }
                function escapeHtml(s) { const d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
                async function setStatus(id, isActive) {
                    const res = await api('/api/businesses/' + id + '/status?isActive=' + isActive, { method: 'PATCH' });
                    if (res.status === 401) { window.location.href = '/admin'; return; }
                    if (res.ok) loadBusinesses();
                }
                document.getElementById('btnCreate').onclick = async () => {
                    const name = document.getElementById('newName').value.trim();
                    const msg = document.getElementById('createMsg');
                    if (!name) { msg.textContent = 'Escribe un nombre.'; msg.className = 'msg err'; return; }
                    msg.textContent = 'Creando...'; msg.className = 'msg';
                    const res = await api('/api/businesses', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name: name }) });
                    if (res.status === 401) { window.location.href = '/admin'; return; }
                    if (res.ok) {
                        msg.textContent = 'Negocio creado.'; msg.className = 'msg ok';
                        document.getElementById('newName').value = '';
                        loadBusinesses();
                    } else {
                        msg.textContent = 'Error al crear.'; msg.className = 'msg err';
                    }
                };
                loadBusinesses().then(renderList);
            </script>
        </body>
        </html>
        """;
}
