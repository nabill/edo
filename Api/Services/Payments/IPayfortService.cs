using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPayfortService
    {
        Task<Result<TokenizationInfo>> Tokenize(TokenizationRequest request);
        Task<Result<PaymentResult>> Pay(PaymentRequest request);
    }
}
