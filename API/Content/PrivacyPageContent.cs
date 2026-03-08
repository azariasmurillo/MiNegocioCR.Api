namespace MiNegocioCR.Api.API.Content;

public static class PrivacyPageContent
{
    public static string Html => """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <title>Política de Privacidad - Mi-NegocioCR</title>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    max-width: 900px;
                    margin: 40px auto;
                    padding: 20px;
                    line-height: 1.6;
                    color: #333;
                }
                h1, h2 {
                    color: #1f2937;
                }
            </style>
        </head>
        <body>
            <h1>Política de Privacidad</h1>
            <p><strong>Última actualización:</strong> Marzo 2026</p>

            <h2>1. Información General</h2>
            <p>
                Mi-NegocioCR es una plataforma SaaS que permite a negocios gestionar la comunicación con sus clientes
                mediante la API oficial de WhatsApp Business proporcionada por Meta.
            </p>

            <h2>2. Información que recopilamos</h2>
            <ul>
                <li>Números de teléfono de clientes</li>
                <li>Mensajes enviados y recibidos a través de WhatsApp</li>
                <li>Información básica del negocio registrada por el usuario</li>
            </ul>

            <h2>3. Uso de la Información</h2>
            <p>
                La información se utiliza exclusivamente para:
            </p>
            <ul>
                <li>Enviar y recibir mensajes mediante WhatsApp Business</li>
                <li>Gestión de órdenes, servicios o soporte</li>
                <li>Mejorar el funcionamiento de la plataforma</li>
            </ul>

            <h2>4. Protección de Datos</h2>
            <p>
                Aplicamos medidas técnicas y organizativas razonables para proteger la información contra
                acceso no autorizado, pérdida o alteración.
            </p>

            <h2>5. Compartición de Información</h2>
            <p>
                Mi-NegocioCR no vende ni comparte datos personales con terceros, excepto cuando sea requerido
                por ley o necesario para el funcionamiento de la API oficial de WhatsApp Business.
            </p>

            <h2>6. Contacto</h2>
            <p>
                Para consultas relacionadas con esta política:
                <br>
                <strong>Email:</strong> soporte@mi-negociocr.com
            </p>

            <p>© 2026 Mi-NegocioCR</p>
        </body>
        </html>
        """;
}
