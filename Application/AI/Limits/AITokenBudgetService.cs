using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Application.AI.Limits
{
    public class AITokenBudgetService
    {
        private readonly AppDbContext _context;

        private const int DailyLimit = 50000;

        public AITokenBudgetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanUseAsync(Guid businessId, int tokens)
        {
            var today = DateTime.UtcNow.Date;

            var usage = await _context.AITokenUsages
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.Date == today);

            if (usage == null)
            {
                usage = new AITokenUsage
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    Date = today,
                    TokensUsed = 0
                };

                _context.AITokenUsages.Add(usage);
            }

            if (usage.TokensUsed + tokens > DailyLimit)
                return false;

            usage.TokensUsed += tokens;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
