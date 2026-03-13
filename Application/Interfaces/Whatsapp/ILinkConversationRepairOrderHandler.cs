using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp;

public interface ILinkConversationRepairOrderHandler
{
    Task Handle(LinkConversationRepairOrderCommandDto command);
}
