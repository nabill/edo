using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IAvailabilityResultsCache
    {
        Task Save(AvailabilityResponse availabilityResponse);
        Task<AvailabilityResponse> Get(int id);
    }
}