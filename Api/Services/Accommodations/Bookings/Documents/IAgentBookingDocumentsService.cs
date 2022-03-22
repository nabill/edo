using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Bookings.Vouchers;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;

public interface IAgentBookingDocumentsService
{
    Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceInfo Data)>> GetActualInvoice(int bookingId, AgentContext agentContext);

    Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId, AgentContext agent, string languageCode);

    Task<Result> SendVoucher(int bookingId, string email, AgentContext agent, string languageCode);

    Task<Result> SendInvoice(int bookingId, string email, AgentContext agent);
}
