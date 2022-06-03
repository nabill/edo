using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Bookings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingStatusHistoryEntry = HappyTravel.Edo.Api.Models.Bookings.BookingStatusHistoryEntry;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingInfoService
    {
        Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, AgentContext agentContext, string languageCode);

        Task<Result<AccommodationBookingInfo>> GetAdministratorAccommodationBookingInfo(string referenceCode, string languageCode);

        Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(string referenceCode, string languageCode);

        Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, AgentContext agentContext, string languageCode);

        IQueryable<SlimAccommodationBookingInfo> GetAgentBookingsInfo(AgentContext agentContext);

        IQueryable<AgentBoundedData<SlimAccommodationBookingInfo>> GetAgencyBookingsInfo(AgentContext agentContext);

        Task<Result<Booking>> GetAgentsBooking(string referenceCode, AgentContext agentContext);

        Task<List<BookingStatusHistoryEntry>> GetBookingStatusHistory(int bookingId);

        Task<List<AdministratorBookingStatusHistoryEntry>> GetAdministratorBookingStatusHistory(int bookingId);

        Task<Result<List<BookingConfirmationHistoryEntry>>> GetBookingConfirmationHistory(string referenceCode);

        Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode, AgentContext? agentContext = null);
    }
}