namespace MiNegocioCR.Api.Application.Interfaces.Business;

public interface IUploadBusinessLogoUseCase
{
    Task<string> Execute(Guid businessId, Stream fileStream, string fileName);
}
