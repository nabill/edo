using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardsManagementService
    {
        TokenizationSettings GetTokenizationSettings();
    }
}