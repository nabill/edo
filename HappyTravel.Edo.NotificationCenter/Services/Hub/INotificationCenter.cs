using System.Threading.Tasks;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public interface INotificationCenter
    {
        Task BookingVoucher(string message);
        Task BookingInvoice(string message);
        Task DeadlineApproaching(string message);
        Task SuccessfulPaymentReceipt(string message);
        Task BookingBuePayment(string message);
        Task BookingCancelled(string message);
        Task BookingFinalized(string message);
    }
}