using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;

namespace HappyTravel.Edo.Api.Services.Payments.Payfort
{
    public interface IPayfortService
    {
        Task<Result<CreditCardPaymentResult>> Authorize(CreditCardPaymentRequest request);

        Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request);

        Task<Result<CreditCardCaptureResult>> Capture(CreditCardCaptureMoneyRequest moneyRequest);

        Task<Result<CreditCardVoidResult>> Void(CreditCardVoidMoneyRequest moneyRequest);
    }
}