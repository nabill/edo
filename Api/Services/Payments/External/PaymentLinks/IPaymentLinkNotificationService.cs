using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public interface IPaymentLinkNotificationService
    {
        Task<Result> SendLink(PaymentLinkData link, string paymentUrl);

        Task<Result> SendPaymentConfirmation(PaymentLinkData link);
    }
}