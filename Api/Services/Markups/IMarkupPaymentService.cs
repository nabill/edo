using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPaymentService
    {
        Task<Result> Pay(string referenceCode);
    }
}