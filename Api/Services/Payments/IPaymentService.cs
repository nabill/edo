using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentService
    {
        IReadOnlyCollection<Currency> GetCurrencies();
        IReadOnlyCollection<PaymentMethod> GetAvailableCustomerPaymentMethods();
    }
}