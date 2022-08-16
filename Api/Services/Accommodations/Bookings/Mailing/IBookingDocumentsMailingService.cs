using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings.Vouchers;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingDocumentsMailingService
    {
        Task<Result> SendVoucher(Booking booking, string email, string languageCode, SlimAgentContext agent);

        Task<Result> SendVoucherPdf(BookingVoucherData voucher, byte[] voucherPdf, string email, SlimAgentContext agent);

        Task<Result> SendInvoice(Booking booking, string email, bool sendCopyToAdmins, SlimAgentContext agent);

        Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email);

        Task<Result> SendPaymentRefundNotification(PaymentRefundMail paymentRefund, string email, SlimAgentContext agent);
    }
}