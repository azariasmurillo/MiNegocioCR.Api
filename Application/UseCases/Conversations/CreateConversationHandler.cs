using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.UseCases.Conversations
{
    public class CreateConversationHandler : ICreateConversationHandler
    {
        private readonly IConversationService _conversationService;

        public CreateConversationHandler(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task Handle(CreateConversationCommandDto command)
        {
            await _conversationService.CreateConversationAsync(
                command.BusinessId,
                command.PhoneNumber,
                command.CustomerName);
        }
    }
}
