using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.Business;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.UseCases.Business
{
    public class CreateBusinessUseCase : ICreateBusinessUseCase
    {
        private readonly IAppDbContext _context;

        public CreateBusinessUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(CreateBusinessRequestDto request)
        {
            var business = new BusinessEntity
            {
                Name = request.Name
            };

            var settings = new BusinessSettings
            {
                Business = business,
                NextOrderNumber = 1
            };

            _context.Businesses.Add(business);
            _context.BusinessSettings.Add(settings);

            await _context.SaveChangesAsync(CancellationToken.None);

            return new
            {
                business.Id,
                business.Name
            };
        }
    }
}
