using MiNegocioCR.Api.Application.Interfaces.ArchiveConversation;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Services;

namespace MiNegocioCR.Api.Application.UseCases.ArchiveConversationUseCase
{
    public class ArchiveConversationUseCase : IArchiveConversationUseCase
    {
        private readonly IWhatsappMessageRepository _repository;

        public ArchiveConversationUseCase(IWhatsappMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid conversationId, bool isArchived)
        {
            await _repository.ArchiveConversationAsync(conversationId, isArchived);
        }
    }
}
