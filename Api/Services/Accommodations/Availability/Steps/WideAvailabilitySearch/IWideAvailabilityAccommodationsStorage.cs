using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilityAccommodationsStorage
    {
        ValueTask EnsureAccommodationsCached(List<string> htIds, string languageCode);

        SlimAccommodation GetAccommodation(string htId, string languageCode);
    }
}