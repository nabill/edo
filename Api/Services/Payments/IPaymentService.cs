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
        Task<Result<List<CardInfo>>> GetAvailableCards();
        Task<Result<PaymentResponse>> NewCardPay(NewCardPaymentRequest request, string languageCode, string ipAddress);
        Task<Result<PaymentResponse>> SavedCardPay(SavedCardPaymentRequest request, string languageCode, string ipAddress);
    }
}
