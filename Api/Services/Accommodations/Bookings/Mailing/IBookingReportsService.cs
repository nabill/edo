using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingReportsService
    {
        Task<Result<string>> SendBookingReports(int agencyId);

        Task<Result> SendBookingsAdministratorSummary();

        Task<Result> SendBookingsPaymentsSummaryToAdministrator();
    }
}