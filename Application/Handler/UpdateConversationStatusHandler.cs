using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.Handler
{
    public class UpdateConversationStatusHandler
    {
        private readonly IConversationService _conversationService;

        public UpdateConversationStatusHandler(
            IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task Handle(UpdateConversationStatusCommandDto command)
        {
            await _conversationService.UpdateStatusAsync(
                command.BusinessId,
                command.PhoneNumber,
                command.Status);
        }
    }
}
