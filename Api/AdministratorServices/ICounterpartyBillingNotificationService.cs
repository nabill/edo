using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyBillingNotificationService
    {
        Task NotifyAdded(int counterpartyId, PaymentData paymentData);
    }
}
