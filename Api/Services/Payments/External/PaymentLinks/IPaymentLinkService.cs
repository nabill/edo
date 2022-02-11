using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public interface IPaymentLinkService
    {
        Task<Result> Send(PaymentLinkCreationRequest paymentLinkCreationData);

        Task<Result<Uri>> GenerateUri(PaymentLinkCreationRequest paymentLinkCreationData);

        Task<Result<PaymentLinkData>> Get(string code);
    }
}