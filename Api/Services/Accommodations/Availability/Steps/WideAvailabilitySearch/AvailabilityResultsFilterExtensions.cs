using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class AvailabilityResultsFilterExtensions
    {
        public static IEnumerable<(int, AccommodationAvailabilityResult)> RemoveRepeatedAccommodations(this IEnumerable<(int, AccommodationAvailabilityResult)> results)
        {
            return results.Distinct(new AccommodationResultComparer());
        }
        
        private class AccommodationResultComparer : IEqualityComparer<(int, AccommodationAvailabilityResult Data)>
        {
            public bool Equals((int, AccommodationAvailabilityResult Data) result1, (int, AccommodationAvailabilityResult Data) result2)
            {
                if (string.IsNullOrWhiteSpace(result1.Data.HtId) || string.IsNullOrWhiteSpace(result2.Data.HtId))
                    return false;
                
                return result1.Data.HtId == result2.Data.HtId;
            }


            public int GetHashCode((int, AccommodationAvailabilityResult Data) result)
            {
                return result.Data.HtId.GetHashCode();
            }
        }
    }
}