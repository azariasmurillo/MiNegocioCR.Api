namespace MiNegocioCR.Api.Domain.Exceptions;

public class NotFoundException : Exception
{
    public const string ErrorCode = "NOT_FOUND";
    public string Resource { get; }

    public NotFoundException(string resource, string message) : base(message)
    {
        Resource = resource;
    }

    /// <summary>
    /// Crea una excepción con solo el mensaje; Resource se establece en "Entity".
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
        Resource = "Entity";
    }
}