using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface IMarkConversationReadHandler
{
    Task Handle(MarkConversationReadCommandDto command);
}
