using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingDocumentsMailingService
    {
        Task<Result> SendVoucher(Booking booking, string email, string languageCode);

        Task<Result> SendInvoice(Booking booking, string email, bool sendCopyToAdmins);
        
        Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email);
    }
}