using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface ICreateConversationHandler
{
    Task Handle(CreateConversationCommandDto command);
}
