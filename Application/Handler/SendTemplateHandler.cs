using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.Handler
{
    public class SendTemplateHandler
    {
        private readonly IQuickReplyService _service;

        public SendTemplateHandler(IQuickReplyService service)
        {
            _service = service;
        }

        public async Task Handle(SendTemplateCommandDto command)
        {
            await _service.SendTemplateAsync(
                command.BusinessId,
                command.Phone,
                command.TemplateId);
        }
    }
}
