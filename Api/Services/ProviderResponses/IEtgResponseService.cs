using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Webhooks.Etg;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public interface IEtgResponseService
    {
        Task<Result> ProcessBookingStatus(EtgBookingResponse bookingResponse);
    }
}