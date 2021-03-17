using System.Threading.Tasks;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public interface INotificationCenter
    {
        Task BookingVoucher(int messageId, string message);
        Task BookingInvoice(int messageId, string message);
        Task DeadlineApproaching(int messageId, string message);
        Task SuccessfulPaymentReceipt(int messageId, string message);
        Task BookingBuePayment(int messageId, string message);
        Task BookingCancelled(int messageId, string message);
        Task BookingFinalized(int messageId, string message);
    }
}