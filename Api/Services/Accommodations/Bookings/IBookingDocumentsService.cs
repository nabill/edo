using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingDocumentsService
    {
        Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId, AgentInfo agent, string languageCode);

        Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceData Data)>> GetActualInvoice(int bookingId, AgentInfo agent, string languageCode);

        Task<Result> GenerateInvoice(int bookingId);
    }
}