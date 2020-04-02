using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardMoneyCaptureService
    {
        Task<Result<CreditCardCaptureResult>> CaptureMoney(CreditCardCaptureMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            Currencies currency,
            CustomerInfo customer);


        Task<Result<CreditCardVoidResult>> VoidMoney(CreditCardVoidMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            MoneyAmount moneyAmount,
            string referenceCode,
            CustomerInfo customer);
    }
}