using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPayfortService
    {
        Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request);

        Result<CreditCardPaymentResult> ProcessPaymentResponse(JObject response);
    }
}
