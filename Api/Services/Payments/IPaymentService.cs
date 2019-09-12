using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentService
    {
        IReadOnlyCollection<Currencies> GetCurrencies();
        IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods();
        Task<Result<PaymentResponse>> PayWithNewCreditCard(PaymentWithNewCreditCardRequest  request, string languageCode, string ipAddress);
        Task<Result<PaymentResponse>> PayWithExistingCard(PaymentWithExistingCreditCardRequest request, string languageCode, string ipAddress);
    }
}
