using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.UseCases.Business
{
    public class SetEnableAIChatUseCase : ISetEnableAIChatUseCase
    {
        private readonly AppDbContext _context;

        public SetEnableAIChatUseCase(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(Guid businessId, bool enable)
        {
            var settings = await _context.BusinessSettings.FindAsync(businessId);

            if (settings == null)
            {
                settings = new Domain.Entities.BusinessSettings
                {
                    BusinessId = businessId,
                    NextOrderNumber = 1,
                    EnableAIChat = enable
                };
                _context.BusinessSettings.Add(settings);
            }
            else
            {
                settings.EnableAIChat = enable;
            }

            await _context.SaveChangesAsync();
        }
    }
}
