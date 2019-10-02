using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPayfortService
    {
        Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request);
    }
}
