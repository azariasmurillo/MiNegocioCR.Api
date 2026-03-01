namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class ConfigureSmtpDto
    {
        public string SmtpHost { get; set; } = default!;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = default!;
        public string SmtpPassword { get; set; } = default!;
        public string SmtpFromEmail { get; set; } = default!;
        public string SmtpFromName { get; set; } = default!;
    }
}
