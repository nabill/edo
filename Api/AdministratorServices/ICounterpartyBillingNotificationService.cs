using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyBillingNotificationService
    {
        Task<Result> NotifyAdded(int counterpartyId, PaymentData paymentData);
    }
}
