using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Mailing;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBookingDocumentsService
    {
        Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId);

        Task<Result<BookingInvoiceData>> GenerateInvoice(int bookingId);
    }
}