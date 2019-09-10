using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPayfortService
    {
        Task<Result<TokenizationInfo>> Tokenization(TokenizationRequest request);
        Task<Result<PaymentInfo>> Payment(PaymentRequest request);
    }
}
