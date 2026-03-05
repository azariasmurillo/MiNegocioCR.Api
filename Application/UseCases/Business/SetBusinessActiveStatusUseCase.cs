using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Business
{
    public class SetBusinessActiveStatusUseCase : ISetBusinessActiveStatusUseCase
    {
        private readonly IAppDbContext _context;

        public SetBusinessActiveStatusUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Execute(Guid businessId, bool isActive)
        {
            var business = await _context.Businesses.FindAsync(businessId);

            if (business == null)
                throw new NotFoundException("Business", "Business not found");

            business.IsActive = isActive;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
