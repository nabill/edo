using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardsManagementService : ICreditCardsManagementService
    {
        public CreditCardsManagementService(EdoContext context, IOptions<PayfortOptions> options)
        {
            _context = context;
            _options = options.Value;
        }


        public TokenizationSettings GetTokenizationSettings() => new TokenizationSettings(_options.AccessCode, _options.Identifier, _options.TokenizationUrl);


        private readonly EdoContext _context;
        private readonly PayfortOptions _options;
    }
}