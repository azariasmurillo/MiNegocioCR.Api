namespace MiNegocioCR.Api.Application.AI.Routing
{
    public class ModelRouter : IModelRouter
    {
        public string SelectModel(string prompt)
        {
            prompt = prompt.ToLower();

            if (prompt.Length < 200)
            {
                return "gpt-4o-mini"; // barato
            }

            return "gpt-4o"; // más potente
        }
    }
}
