using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public interface IBookingMailingService
    {
        Task<Result> SendVoucher(int bookingId, string email, AgentContext agent, string languageCode);

        Task<Result> SendInvoice(int bookingId, string email, int agentId);

        Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo);

        Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo);

        Task<Result> NotifyDeadlineApproaching(int bookingId, string email);

        Task<Result<string>> SendBookingReports(int agencyId);

        Task<Result> SendBookingsAdministratorSummary();
        
        Task<Result> SendBookingsPaymentsSummaryToAdministrator();

        Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email);

        Task<Result> SendCreditCardPaymentNotifications(string referenceCode);
    }
}