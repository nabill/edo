using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusClient {
        Task<Result<NGeniusPaymentResponse>> CreateOrder(string orderType, string referenceCode, Currencies currency, decimal price, string email,
            NGeniusBillingAddress billingAddress);
        
        Task<Result<string>> CaptureMoney(string paymentId, string orderReference, MoneyAmount amount);

        Task<Result> VoidMoney(string paymentId, string orderReference, Currencies currency);

        Task<Result> RefundMoney(string paymentId, string orderReference, string captureId, MoneyAmount amount);

        Task<Result<PaymentStatuses>> GetStatus(string orderReference, Currencies currency);
    }
}