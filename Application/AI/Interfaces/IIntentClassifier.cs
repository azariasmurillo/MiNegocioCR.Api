using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IIntentClassifier
    {
        AIIntent Classify(string message);
    }
}
