namespace MiNegocioCR.Api.Domain.Exceptions;

public class NotFoundException : Exception
{
    public const string ErrorCode = "NOT_FOUND";
    public string Resource { get; }

    public NotFoundException(string resource, string message) : base(message)
    {
        Resource = resource;
    }
}