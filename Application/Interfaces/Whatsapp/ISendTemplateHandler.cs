using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface ISendTemplateHandler
{
    Task Handle(SendTemplateCommandDto command);
}
