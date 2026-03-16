using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class QuickReplyService : IQuickReplyService
    {
        private readonly IAppDbContext _context;
        private readonly IWhatsappApplicationService _whatsappApplicationService;

        public QuickReplyService(IAppDbContext context,
            IWhatsappApplicationService whatsappApplicationService)
        {
            _context = context;
            _whatsappApplicationService = whatsappApplicationService;
        }

        public async Task SendTemplateAsync(Guid businessId,string phone,Guid templateId)
        {
            var template = await _context.QuickReplyTemplates
                .FirstOrDefaultAsync(x =>
                    x.Id == templateId &&
                    x.BusinessId == businessId);

            if (template == null)
                throw new NotFoundException("Template not found");

            await _whatsappApplicationService.SendAsync(
                businessId,
                phone,
                template.MessageText);
        }
    }
}
