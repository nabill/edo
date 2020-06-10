using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public interface IBookingMailingService
    {
        Task<Result> SendVoucher(int bookingId, string email, AgentInfo agent, string languageCode);

        Task<Result> SendInvoice(int bookingId, string email, AgentInfo agent, string languageCode);

        Task<Result> NotifyBookingCancelled(string referenceCode, string email, string agentName);

        Task<Result> NotifyDeadlineApproaching(int bookingId, string email);
    }
}