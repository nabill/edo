using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface IServicePaymentsService
    {
        Task<Result<MoneyAmount>> GetServicePrice(string referenceCode);

        Task<Result> ProcessPaymentChanges(Payment payment);
    }
}