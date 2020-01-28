using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface IMultiProviderAvailabilityManager
    {
        Task<Result<AvailabilityDetails>> GetAvailability(AvailabilityRequest availabilityRequest, string languageCode);
    }
}