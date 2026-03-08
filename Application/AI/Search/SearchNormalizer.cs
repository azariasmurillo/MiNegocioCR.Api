namespace MiNegocioCR.Api.Application.AI.Search
{
    public class SearchNormalizer
    {
        private static readonly HashSet<string> StopWords = new()
        {
            "hola","mae","me","regalas","regalan","tienen","tiene","busco","ocupó","ocupo","quiero"
        };

        private static readonly Dictionary<string, string> Synonyms = new()
        {
            {"fundas","funda"},
            {"protector","funda"},
            {"protectores","funda"},

            {"cargadores","cargador"},
            {"adaptador","cargador"},
            {"adaptadores","cargador"},

            {"memoria","ram"},
            {"memorias","ram"},

            {"disco","ssd"},
            {"discos","ssd"},

            {"mouse","mouse"},
            {"mouses","mouse"},

            {"teclados","teclado"},
            {"teclado gamer","teclado"},

            {"pantalla","display"},
            {"pantallas","display"},

            {"baterias","bateria"},
            {"batería","bateria"},

            {"audifonos","audifonos"},
            {"audífonos","audifonos"},
            {"headset","audifonos"},

            {"usb","memoria usb"},
            {"flash","memoria usb"},
        };

        public List<string> Normalize(string query)
        {
            query = query.ToLower().Trim();

            var words = query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(StripPunctuation)
                .Where(w => w.Length > 0 && !StopWords.Contains(w))
                .ToList();

            var result = new List<string>();

            foreach (var word in words)
            {
                if (Synonyms.TryGetValue(word, out var synonym))
                    result.Add(synonym);
                else
                    result.Add(word);
            }

            return result;
        }

        private static string StripPunctuation(string word)
        {
            var chars = word.Where(c => !char.IsPunctuation(c)).ToArray();
            return new string(chars);
        }
    }
}
