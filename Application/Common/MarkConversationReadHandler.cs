using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.Common
{
    public class MarkConversationReadHandler : IMarkConversationReadHandler
    {
        private readonly IConversationService _conversationService;

        public MarkConversationReadHandler(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task Handle(MarkConversationReadCommandDto command)
        {
            await _conversationService.MarkConversationReadAsync(
                command.BusinessId,
                command.ConversationId);
        }
    }
}
