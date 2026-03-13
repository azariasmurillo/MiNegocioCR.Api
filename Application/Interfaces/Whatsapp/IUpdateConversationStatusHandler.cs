using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface IUpdateConversationStatusHandler
{
    Task Handle(UpdateConversationStatusCommandDto command);
}
