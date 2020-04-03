using System.Collections.Generic;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentSettingsService
    {
        IReadOnlyCollection<Currencies> GetCurrencies();

        IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods();
    }
}