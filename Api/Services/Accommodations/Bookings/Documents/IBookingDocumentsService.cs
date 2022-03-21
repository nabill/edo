using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Bookings.Vouchers;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;

public interface IBookingDocumentsService
{
    Task<Result<BookingVoucherData>> GenerateVoucher(Data.Bookings.Booking booking, string languageCode);

    Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceInfo Data)>> GetActualInvoice(Data.Bookings.Booking booking);

    Task<Result> GenerateInvoice(Data.Bookings.Booking booking);

    Task<Result<(DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data)>> GenerateReceipt(Data.Bookings.Booking booking);
}
