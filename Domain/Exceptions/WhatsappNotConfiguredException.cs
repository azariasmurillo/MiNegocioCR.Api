namespace MiNegocioCR.Api.Domain.Exceptions
{
    public class WhatsappNotConfiguredException : Exception
    {
        public const string ErrorCode = "WHATSAPP_NOT_CONFIGURED";

        public WhatsappNotConfiguredException()
            : base("WhatsApp is not configured for this business. Connect WhatsApp first via the connect flow.")
        {
        }
    }
}
