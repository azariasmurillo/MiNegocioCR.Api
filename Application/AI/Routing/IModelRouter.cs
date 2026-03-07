namespace MiNegocioCR.Api.Application.AI.Routing
{
    public interface IModelRouter
    {
        string SelectModel(string prompt);
    }
}
