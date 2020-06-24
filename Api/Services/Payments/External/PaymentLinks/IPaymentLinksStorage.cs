using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Data.PaymentLinks;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public interface IPaymentLinksStorage
    {
        Task<Result<PaymentLink>> Get(string referenceCode);

        Task<Result<PaymentLink>> StoreLink(CreatePaymentLinkRequest paymentLinkData);

        Task<Result> UpdatePaymentStatus(string code, PaymentResponse paymentResponse);
    }
}