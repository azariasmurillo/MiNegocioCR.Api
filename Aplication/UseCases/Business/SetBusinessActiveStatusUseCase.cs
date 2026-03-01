using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.Business;

namespace MiNegocioCR.Api.Aplication.UseCases.Business
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
                throw new Exception("Business not found");

            business.IsActive = isActive;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
