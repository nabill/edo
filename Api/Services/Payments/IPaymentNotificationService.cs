using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentNotificationService
    {
        Task<Result> SendBillToCustomer(PaymentBill paymentBill);
        Task<Result> SendNeedPaymentNotificationToCustomer(PaymentBill paymentBill);
    }
}