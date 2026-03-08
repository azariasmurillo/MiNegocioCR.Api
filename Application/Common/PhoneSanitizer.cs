namespace MiNegocioCR.Api.Application.Common
{
    public static class PhoneSanitizer
    {
        /// <summary>
        /// Quita el símbolo '+' y espacios del teléfono (ej. "+506 8888 1234" → "50688881234").
        /// </summary>
        public static string? Sanitize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;
            return phone.Replace("+", "").Trim();
        }
    }
}
