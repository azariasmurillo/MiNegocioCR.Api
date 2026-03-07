namespace MiNegocioCR.Api.Application.AI.Guardrails
{
    public class DomainFilter
    {
        private static readonly string[] AllowedKeywords =
 {
    "precio",
    "producto",
    "productos",
    "reparacion",
    "reparar",
    "stock",
    "inventario",
    "orden",
    "servicio",
    "ssd",
    "memoria",
    "laptop",
    "cargador",
    "funda",
    "pantalla",
    "bateria",
    "tienen",
    "cuanto"
};

        public bool IsAllowed(string message)
        {
            var text = message.ToLower();

            return AllowedKeywords.Any(k => text.Contains(k));
        }
    }
}
