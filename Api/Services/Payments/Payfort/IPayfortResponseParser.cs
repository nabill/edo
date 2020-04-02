using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.Payfort
{
    public interface IPayfortResponseParser
    {
        Result<CreditCardPaymentResult> ParsePaymentResponse(JObject response);

        Result<T> ParseResponse<T>(JObject response);
    }
}