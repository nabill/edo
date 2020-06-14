using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentNotificationService
    {
        Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email);
    }
}