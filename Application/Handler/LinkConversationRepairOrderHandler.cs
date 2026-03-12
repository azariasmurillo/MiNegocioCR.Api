using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.Handler
{
    public class LinkConversationRepairOrderHandler
    {
        private readonly IConversationService _conversationService;

        public LinkConversationRepairOrderHandler(
            IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task Handle(LinkConversationRepairOrderCommandDto command)
        {
            await _conversationService.LinkRepairOrderAsync(
                command.BusinessId,
                command.PhoneNumber,
                command.RepairOrderId);
        }
    }
}
